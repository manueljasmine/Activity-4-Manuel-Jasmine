using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace jasminelogin
{
    public partial class DashboardForm : Form
    {
        private readonly string fullName;
        private readonly Color green = Color.FromArgb(35, 139, 82);
        private readonly Color darkGreen = Color.FromArgb(20, 90, 50);
        private readonly Color pageGreen = Color.FromArgb(240, 250, 244);
        private readonly TextBox txtProductName = new();
        private readonly TextBox txtDescription = new();
        private readonly TextBox txtUnitPrice = new();
        private readonly TextBox txtQuantity = new();
        private readonly TextBox txtReorderLevel = new();
        private readonly TextBox txtSearch = new();
        private readonly DataGridView productGrid = new();
        private readonly Label lblTotal = new();
        private readonly Label lblInStock = new();
        private readonly Label lblLowStock = new();
        private readonly Label lblOutOfStock = new();
        private int selectedProductId;

        public DashboardForm(string fullName)
        {
            this.fullName = fullName;
            Text = "Login - Dashboard";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1050, 700);
            Size = new Size(1180, 780);
            BackColor = pageGreen;
            Font = new Font("Segoe UI", 9F);
            BuildDashboard();
            LoadProducts();
        }

        private void BuildDashboard()
        {
            Panel header = new() { Dock = DockStyle.Top, Height = 66, BackColor = darkGreen, Padding = new Padding(22, 0, 22, 0) };
            Label welcome = new() { Text = "Welcome, " + fullName + "!", ForeColor = Color.White, Font = new Font("Segoe UI", 15, FontStyle.Bold), AutoSize = true, Location = new Point(22, 18) };
            Button logout = MakeButton("LOGOUT", Color.White, darkGreen, 105, 34);
            logout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            logout.Location = new Point(Width - 155, 16);
            logout.Click += (_, _) => Close();
            header.Controls.Add(welcome);
            header.Controls.Add(logout);
            header.Resize += (_, _) => logout.Location = new Point(header.ClientSize.Width - 130, 16);
            Controls.Add(header);

            Panel content = new() { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(22, 16, 22, 18), BackColor = pageGreen };
            Controls.Add(content);
            header.BringToFront();

            Label heading = new() { Text = "Product Management", Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = darkGreen, AutoSize = true, Location = new Point(22, 12) };
            Label subheading = new() { Text = "Manage your inventory products", ForeColor = Color.DimGray, AutoSize = true, Location = new Point(24, 44) };
            content.Controls.Add(heading);
            content.Controls.Add(subheading);

            FlowLayoutPanel cards = new() { Location = new Point(22, 78), Size = new Size(1080, 100), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, WrapContents = false, BackColor = pageGreen };
            cards.Controls.Add(CreateSummaryCard("TOTAL PRODUCTS", lblTotal, Color.FromArgb(190, 216, 235)));
            cards.Controls.Add(CreateSummaryCard("IN STOCK", lblInStock, Color.FromArgb(167, 224, 183)));
            cards.Controls.Add(CreateSummaryCard("LOW STOCK", lblLowStock, Color.FromArgb(255, 218, 160)));
            cards.Controls.Add(CreateSummaryCard("OUT OF STOCK", lblOutOfStock, Color.FromArgb(245, 170, 178)));
            content.Controls.Add(cards);

            GroupBox editor = new() { Text = " Product Information ", ForeColor = darkGreen, Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(22, 190), Size = new Size(1080, 175), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.White };
            content.Controls.Add(editor);
            AddInput(editor, "Product Name", txtProductName, 18, 28, 260, 30);
            AddInput(editor, "Description", txtDescription, 18, 91, 260, 48, true);
            AddInput(editor, "Unit Price", txtUnitPrice, 310, 28, 230, 30);
            AddInput(editor, "Quantity", txtQuantity, 310, 91, 230, 30);
            AddInput(editor, "Reorder Level", txtReorderLevel, 585, 28, 220, 30);

            FlowLayoutPanel actions = new() { Location = new Point(835, 27), Size = new Size(220, 125), FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.White };
            Button add = MakeButton("ADD PRODUCT", green, Color.White, 190, 29);
            Button update = MakeButton("UPDATE", Color.FromArgb(42, 130, 190), Color.White, 190, 29);
            Button delete = MakeButton("DELETE", Color.FromArgb(205, 65, 65), Color.White, 190, 29);
            Button clear = MakeButton("CLEAR", Color.FromArgb(235, 235, 235), darkGreen, 190, 29);
            add.Click += AddProduct_Click;
            update.Click += UpdateProduct_Click;
            delete.Click += DeleteProduct_Click;
            clear.Click += (_, _) => ClearFields();
            actions.Controls.Add(add); actions.Controls.Add(update); actions.Controls.Add(delete); actions.Controls.Add(clear);
            editor.Controls.Add(actions);

            Panel listPanel = new() { Location = new Point(22, 380), Size = new Size(1080, 300), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, BackColor = Color.White, Padding = new Padding(12) };
            content.Controls.Add(listPanel);
            Label listTitle = new() { Text = "Product List", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = darkGreen, AutoSize = true, Location = new Point(12, 10) };
            listPanel.Controls.Add(listTitle);
            Label searchLabel = new() { Text = "Search", AutoSize = true, ForeColor = Color.DimGray, Location = new Point(680, 15) };
            txtSearch.Location = new Point(735, 10); txtSearch.Size = new Size(315, 27); txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtSearch.TextChanged += (_, _) => LoadProducts();
            listPanel.Controls.Add(searchLabel); listPanel.Controls.Add(txtSearch);

            productGrid.Location = new Point(12, 48);
            productGrid.Size = new Size(1040, 225);
            productGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            productGrid.ReadOnly = true;
            productGrid.AllowUserToAddRows = false;
            productGrid.AllowUserToDeleteRows = false;
            productGrid.MultiSelect = false;
            productGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            productGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            productGrid.BackgroundColor = Color.White;
            productGrid.BorderStyle = BorderStyle.FixedSingle;
            productGrid.RowHeadersVisible = false;
            productGrid.EnableHeadersVisualStyles = false;
            productGrid.ColumnHeadersDefaultCellStyle.BackColor = darkGreen;
            productGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            productGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            productGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(246, 251, 247);
            productGrid.SelectionChanged += ProductGrid_SelectionChanged;
            listPanel.Controls.Add(productGrid);
        }

        private Panel CreateSummaryCard(string title, Label valueLabel, Color color)
        {
            Panel card = new() { Size = new Size(245, 88), BackColor = color, Margin = new Padding(0, 0, 12, 0), Padding = new Padding(10) };
            Label caption = new() { Text = title, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = darkGreen, AutoSize = true, Location = new Point(12, 12) };
            valueLabel.Text = "0"; valueLabel.Font = new Font("Segoe UI", 22, FontStyle.Bold); valueLabel.ForeColor = Color.FromArgb(35, 45, 40); valueLabel.AutoSize = true; valueLabel.Location = new Point(12, 34);
            card.Controls.Add(caption); card.Controls.Add(valueLabel);
            return card;
        }

        private static void AddInput(Control parent, string caption, TextBox box, int x, int y, int width, int height, bool multiline = false)
        {
            Label label = new() { Text = caption, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(65, 75, 68) };
            box.Location = new Point(x, y + 21); box.Size = new Size(width, height); box.Font = new Font("Segoe UI", 9.5F); box.BorderStyle = BorderStyle.FixedSingle; box.Multiline = multiline;
            parent.Controls.Add(label); parent.Controls.Add(box);
        }

        private static Button MakeButton(string text, Color back, Color fore, int width, int height)
        {
            return new Button { Text = text, Size = new Size(width, height), BackColor = back, ForeColor = fore, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 0, 5) };
        }

        private void LoadProducts()
        {
            try
            {
                DataTable products = DatabaseHelper.GetProducts(txtSearch.Text);
                productGrid.DataSource = products;
                if (productGrid.Columns.Contains("ProductID")) productGrid.Columns["ProductID"].HeaderText = "ID";
                if (productGrid.Columns.Contains("ProductName")) productGrid.Columns["ProductName"].HeaderText = "Product Name";
                if (productGrid.Columns.Contains("UnitPrice")) { productGrid.Columns["UnitPrice"].HeaderText = "Unit Price"; productGrid.Columns["UnitPrice"].DefaultCellStyle.Format = "₱#,##0.00"; }
                if (productGrid.Columns.Contains("ReorderLevel")) productGrid.Columns["ReorderLevel"].HeaderText = "Reorder Level";
                var summary = DatabaseHelper.GetStockSummary();
                lblTotal.Text = summary.Total.ToString(); lblInStock.Text = summary.InStock.ToString();
                lblLowStock.Text = summary.LowStock.ToString(); lblOutOfStock.Text = summary.OutOfStock.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load products.\n\n" + ex.Message + "\n\nDatabase: " + DatabaseHelper.GetDatabasePath(), "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateProduct(out decimal price, out int quantity, out int reorder)
        {
            price = 0; quantity = 0; reorder = 0;
            if (string.IsNullOrWhiteSpace(txtProductName.Text)) { MessageBox.Show("Please enter the product name."); txtProductName.Focus(); return false; }
            if (txtProductName.Text.Trim().Length > 100) { MessageBox.Show("Product name cannot exceed 100 characters."); return false; }
            if (!decimal.TryParse(txtUnitPrice.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out price) || price < 0) { MessageBox.Show("Enter a valid non-negative unit price."); txtUnitPrice.Focus(); return false; }
            if (!int.TryParse(txtQuantity.Text.Trim(), out quantity) || quantity < 0) { MessageBox.Show("Quantity must be a whole number (0 or higher)."); txtQuantity.Focus(); return false; }
            if (!int.TryParse(txtReorderLevel.Text.Trim(), out reorder) || reorder < 0) { MessageBox.Show("Reorder level must be a whole number (0 or higher)."); txtReorderLevel.Focus(); return false; }
            return true;
        }

        private void AddProduct_Click(object? sender, EventArgs e)
        {
            if (!ValidateProduct(out decimal price, out int quantity, out int reorder)) return;
            try { DatabaseHelper.AddProduct(txtProductName.Text, txtDescription.Text, price, quantity, reorder); LoadProducts(); ClearFields(); MessageBox.Show("Product added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            catch (Exception ex) { ShowDatabaseError(ex); }
        }

        private void UpdateProduct_Click(object? sender, EventArgs e)
        {
            if (selectedProductId <= 0) { MessageBox.Show("Please select a product from the list to update."); return; }
            if (!ValidateProduct(out decimal price, out int quantity, out int reorder)) return;
            try { DatabaseHelper.UpdateProduct(selectedProductId, txtProductName.Text, txtDescription.Text, price, quantity, reorder); LoadProducts(); ClearFields(); MessageBox.Show("Product updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            catch (Exception ex) { ShowDatabaseError(ex); }
        }

        private void DeleteProduct_Click(object? sender, EventArgs e)
        {
            if (selectedProductId <= 0) { MessageBox.Show("Please select a product from the list to delete."); return; }
            if (MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { DatabaseHelper.DeleteProduct(selectedProductId); LoadProducts(); ClearFields(); MessageBox.Show("Product deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            catch (Exception ex) { ShowDatabaseError(ex); }
        }

        private void ProductGrid_SelectionChanged(object? sender, EventArgs e)
        {
            if (productGrid.CurrentRow == null || productGrid.CurrentRow.IsNewRow) return;
            DataGridViewRow row = productGrid.CurrentRow;
            if (row.Cells["ProductID"].Value == null) return;
            selectedProductId = Convert.ToInt32(row.Cells["ProductID"].Value);
            txtProductName.Text = Convert.ToString(row.Cells["ProductName"].Value) ?? "";
            txtDescription.Text = Convert.ToString(row.Cells["Description"].Value) ?? "";
            txtUnitPrice.Text = Convert.ToDecimal(row.Cells["UnitPrice"].Value).ToString("0.00", CultureInfo.CurrentCulture);
            txtQuantity.Text = Convert.ToString(row.Cells["Quantity"].Value) ?? "0";
            txtReorderLevel.Text = Convert.ToString(row.Cells["ReorderLevel"].Value) ?? "0";
        }

        private void ClearFields()
        {
            selectedProductId = 0;
            txtProductName.Clear(); txtDescription.Clear(); txtUnitPrice.Clear(); txtQuantity.Clear(); txtReorderLevel.Clear();
            productGrid.ClearSelection();
            txtProductName.Focus();
        }

        private static void ShowDatabaseError(Exception ex) => MessageBox.Show("The operation could not be completed.\n\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
