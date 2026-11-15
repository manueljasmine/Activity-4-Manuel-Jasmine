using System;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace jasminelogin
{
    public static class DatabaseHelper
    {
        private const string DatabaseFileName = "JasmineLogin.accdb";

        private static string DatabasePath => ResolveDatabasePath();

        private static string ConnectionString =>
            $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={DatabasePath};Persist Security Info=False;";

        public static string GetDatabasePath() => DatabasePath;

        private static string ResolveDatabasePath()
        {
            DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, DatabaseFileName);
                bool isProjectFolder = Directory.GetFiles(directory.FullName, "*.csproj").Length > 0;

                if (isProjectFolder && File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }

            string outputCopy = Path.Combine(AppContext.BaseDirectory, DatabaseFileName);
            if (File.Exists(outputCopy))
                return outputCopy;

            throw new FileNotFoundException(
                "JasmineLogin.accdb was not found. Place it in the project folder or beside the application.",
                outputCopy);
        }

        public static OleDbConnection GetConnection()
        {
            return new OleDbConnection(ConnectionString);
        }

        public static bool TestConnection()
        {
            using OleDbConnection connection = GetConnection();
            connection.Open();

            using OleDbCommand command =
                new OleDbCommand("SELECT COUNT(*) FROM [AppUsers]", connection);
            command.ExecuteScalar();

            EnsureProductsTable(connection);
            return true;
        }

        private static void EnsureProductsTable(OleDbConnection connection)
        {
            DataTable tables = connection.GetSchema("Tables");

            foreach (DataRow row in tables.Rows)
            {
                string tableName = Convert.ToString(row["TABLE_NAME"]) ?? string.Empty;

                if (string.Equals(tableName, "Products", StringComparison.OrdinalIgnoreCase))
                    return;
            }

            const string sql = @"
                CREATE TABLE [Products] (
                    [ProductID] COUNTER CONSTRAINT [PK_Products] PRIMARY KEY,
                    [ProductName] TEXT(100) NOT NULL,
                    [Description] MEMO,
                    [UnitPrice] CURRENCY NOT NULL,
                    [Quantity] LONG NOT NULL,
                    [ReorderLevel] LONG NOT NULL
                )";

            using OleDbCommand createCommand = new OleDbCommand(sql, connection);
            createCommand.ExecuteNonQuery();
        }

        private static void EnsureProductsTable()
        {
            using OleDbConnection connection = GetConnection();
            connection.Open();
            EnsureProductsTable(connection);
        }

        // LOGIN AND REGISTRATION METHODS
        public static bool UsernameExists(string username)
        {
            using OleDbConnection connection = GetConnection();
            connection.Open();

            using OleDbCommand command = new OleDbCommand(
                "SELECT COUNT(*) FROM [AppUsers] WHERE [Username] = ?", connection);

            command.Parameters.Add("?", OleDbType.VarWChar, 50).Value = username.Trim();
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public static bool RegisterUser(string fullName, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Please complete all registration fields.");
            }

            using OleDbConnection connection = GetConnection();
            connection.Open();

            if (UsernameExistsInConnection(connection, username))
                return false;

            string passwordHash = PasswordHasher.HashPassword(password);

            using OleDbCommand command = new OleDbCommand(
                "INSERT INTO [AppUsers] ([FullName], [Username], [PasswordHash]) VALUES (?, ?, ?)",
                connection);

            command.Parameters.Add("?", OleDbType.VarWChar, 100).Value = fullName.Trim();
            command.Parameters.Add("?", OleDbType.VarWChar, 50).Value = username.Trim();
            command.Parameters.Add("?", OleDbType.VarWChar, 255).Value = passwordHash;

            return command.ExecuteNonQuery() == 1;
        }

        private static bool UsernameExistsInConnection(OleDbConnection connection, string username)
        {
            using OleDbCommand command = new OleDbCommand(
                "SELECT COUNT(*) FROM [AppUsers] WHERE [Username] = ?", connection);

            command.Parameters.Add("?", OleDbType.VarWChar, 50).Value = username.Trim();
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public static (string FullName, string PasswordHash)? FindUser(string username)
        {
            using OleDbConnection connection = GetConnection();
            connection.Open();

            using OleDbCommand command = new OleDbCommand(
                "SELECT [FullName], [PasswordHash] FROM [AppUsers] WHERE [Username] = ?",
                connection);

            command.Parameters.Add("?", OleDbType.VarWChar, 50).Value = username.Trim();

            using OleDbDataReader reader = command.ExecuteReader();

            if (reader != null && reader.Read())
            {
                string fullName = reader["FullName"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(reader["FullName"]) ?? string.Empty;

                string passwordHash = reader["PasswordHash"] == DBNull.Value
                    ? string.Empty
                    : Convert.ToString(reader["PasswordHash"]) ?? string.Empty;

                return (fullName, passwordHash);
            }

            return null;
        }

        // PRODUCT CRUD METHODS
        public static DataTable GetProducts(string search = "")
        {
            EnsureProductsTable();

            using OleDbConnection connection = GetConnection();
            connection.Open();

            string sql =
                "SELECT [ProductID], [ProductName], [Description], [UnitPrice], [Quantity], [ReorderLevel] " +
                "FROM [Products]";

            using OleDbCommand command = new OleDbCommand();
            command.Connection = connection;

            if (!string.IsNullOrWhiteSpace(search))
            {
                sql += " WHERE InStr(1, [ProductName], ?) > 0";
                command.Parameters.Add("?", OleDbType.VarWChar, 255).Value = search.Trim();
            }

            sql += " ORDER BY [ProductID] DESC";
            command.CommandText = sql;

            DataTable table = new DataTable();
            using OleDbDataAdapter adapter = new OleDbDataAdapter(command);
            adapter.Fill(table);

            return table;
        }

        public static int AddProduct(
            string name,
            string description,
            decimal price,
            int quantity,
            int reorderLevel)
        {
            ValidateProductValues(name, price, quantity, reorderLevel);
            EnsureProductsTable();

            using OleDbConnection connection = GetConnection();
            connection.Open();

            using OleDbCommand command = new OleDbCommand(
                "INSERT INTO [Products] ([ProductName], [Description], [UnitPrice], [Quantity], [ReorderLevel]) " +
                "VALUES (?, ?, ?, ?, ?)", connection);

            AddProductParameters(command, name, description, price, quantity, reorderLevel);
            return command.ExecuteNonQuery();
        }

        public static int UpdateProduct(
            int id,
            string name,
            string description,
            decimal price,
            int quantity,
            int reorderLevel)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Select a valid product first.");

            ValidateProductValues(name, price, quantity, reorderLevel);
            EnsureProductsTable();

            using OleDbConnection connection = GetConnection();
            connection.Open();

            using OleDbCommand command = new OleDbCommand(
                "UPDATE [Products] SET [ProductName]=?, [Description]=?, [UnitPrice]=?, " +
                "[Quantity]=?, [ReorderLevel]=? WHERE [ProductID]=?", connection);

            AddProductParameters(command, name, description, price, quantity, reorderLevel);
            command.Parameters.Add("?", OleDbType.Integer).Value = id;

            return command.ExecuteNonQuery();
        }

        private static void AddProductParameters(
            OleDbCommand command,
            string name,
            string description,
            decimal price,
            int quantity,
            int reorderLevel)
        {
            command.Parameters.Add("?", OleDbType.VarWChar, 100).Value = name.Trim();
            command.Parameters.Add("?", OleDbType.LongVarWChar).Value =
                description?.Trim() ?? string.Empty;
            command.Parameters.Add("?", OleDbType.Currency).Value = price;
            command.Parameters.Add("?", OleDbType.Integer).Value = quantity;
            command.Parameters.Add("?", OleDbType.Integer).Value = reorderLevel;
        }

        private static void ValidateProductValues(
            string name,
            decimal price,
            int quantity,
            int reorderLevel)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required.", nameof(name));

            if (name.Trim().Length > 100)
                throw new ArgumentException("Product name cannot exceed 100 characters.", nameof(name));

            if (price < 0)
                throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

            if (quantity < 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");

            if (reorderLevel < 0)
                throw new ArgumentOutOfRangeException(nameof(reorderLevel), "Reorder level cannot be negative.");
        }

        public static int DeleteProduct(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id), "Select a valid product first.");

            EnsureProductsTable();

            using OleDbConnection connection = GetConnection();
            connection.Open();

            using OleDbCommand command = new OleDbCommand(
                "DELETE FROM [Products] WHERE [ProductID] = ?", connection);

            command.Parameters.Add("?", OleDbType.Integer).Value = id;
            return command.ExecuteNonQuery();
        }

        public static (int Total, int InStock, int LowStock, int OutOfStock) GetStockSummary()
        {
            EnsureProductsTable();

            using OleDbConnection connection = GetConnection();
            connection.Open();

            const string sql =
                "SELECT COUNT(*) AS TotalProducts, " +
                "SUM(IIF([Quantity] > [ReorderLevel], 1, 0)) AS InStock, " +
                "SUM(IIF([Quantity] > 0 AND [Quantity] <= [ReorderLevel], 1, 0)) AS LowStock, " +
                "SUM(IIF([Quantity] <= 0, 1, 0)) AS OutOfStock " +
                "FROM [Products]";

            using OleDbCommand command = new OleDbCommand(sql, connection);
            using OleDbDataReader reader = command.ExecuteReader();

            if (reader != null && reader.Read())
            {
                return (
                    ToInt(reader["TotalProducts"]),
                    ToInt(reader["InStock"]),
                    ToInt(reader["LowStock"]),
                    ToInt(reader["OutOfStock"]));
            }

            return (0, 0, 0, 0);
        }

        private static int ToInt(object value)
        {
            return value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }
    }
}
