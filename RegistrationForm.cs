
using System;
using System.Drawing;
using System.Windows.Forms;

namespace jasminelogin
{
    public partial class RegistrationForm : Form
    {
        private readonly TextBox txtFullName = new();
        private readonly TextBox txtUsername = new();
        private readonly TextBox txtPassword = new();
        private readonly TextBox txtConfirmPassword = new();

        public RegistrationForm()
        {
            Text = "Login - Registration";
            Size = new Size(450, 620);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(240, 250, 244);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Label title = new Label
            {
                Text = "CREATE ACCOUNT",
                Font = new Font("Segoe UI", 21, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 90, 50),
                Location = new Point(45, 30),
                Size = new Size(350, 45),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Controls.Add(title);

            AddField("Full Name", txtFullName, 100);
            AddField("Username", txtUsername, 195);
            AddField("Password", txtPassword, 290);
            AddField("Confirm Password", txtConfirmPassword, 385);

            txtFullName.MaxLength = 100;
            txtUsername.MaxLength = 50;
            txtPassword.MaxLength = 256;
            txtConfirmPassword.MaxLength = 256;

            txtPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.UseSystemPasswordChar = true;

            Button btnRegister = CreateButton(
                "REGISTER", 105, 490);

            btnRegister.Click += Register_Click;

            Controls.Add(btnRegister);
        }

        // CREATE INPUT FIELDS
        private void AddField(
            string labelText,
            TextBox textBox,
            int top)
        {
            Label label = new Label
            {
                Text = labelText,
                Location = new Point(55, top),
                Size = new Size(320, 22),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(20, 90, 50)
            };

            textBox.Location = new Point(55, top + 27);
            textBox.Size = new Size(320, 32);
            textBox.Font = new Font("Segoe UI", 11);

            Controls.Add(label);
            Controls.Add(textBox);
        }

        // CREATE BUTTON
        private Button CreateButton(
            string text,
            int x,
            int y)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(220, 44),
                BackColor = Color.FromArgb(35, 139, 82),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        // REGISTER BUTTON
        private void Register_Click(object? sender, EventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConfirmPassword.Text;

            // CHECK EMPTY FIELDS
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirm))
            {
                MessageBox.Show(
                    "Please complete all fields.",
                    "Login",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // CHECK FIELD LENGTH
            if (fullName.Length > 100 || username.Length > 50)
            {
                MessageBox.Show(
                    "Full Name or Username is too long."
                );
                return;
            }

            // CHECK PASSWORD LENGTH
            if (password.Length < 8 || password.Length > 256)
            {
                MessageBox.Show(
                    "Password must contain 8 to 256 characters."
                );
                return;
            }

            // CHECK PASSWORD MATCH
            if (password != confirm)
            {
                MessageBox.Show(
                    "Passwords do not match!",
                    "Login",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            try
            {
                // TEST DATABASE CONNECTION
                if (!DatabaseHelper.TestConnection())
                {
                    MessageBox.Show(
                        "Database connection failed."
                    );
                    return;
                }

                // CHECK IF USERNAME ALREADY EXISTS
                if (DatabaseHelper.UsernameExists(username))
                {
                    MessageBox.Show(
                        "Username already exists. Please choose another.",
                        "Login",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    txtUsername.Focus();
                    return;
                }

                // SAVE USER
                bool result = DatabaseHelper.RegisterUser(
                    fullName,
                    username,
                    password
                );

                if (result)
                {
                    MessageBox.Show(
                        "Registration Successful!\n\n" +
                        "Welcome to Login, " + fullName + "!",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    txtFullName.Clear();
                    txtUsername.Clear();
                    txtPassword.Clear();
                    txtConfirmPassword.Clear();

                    Close();
                }
                else
                {
                    MessageBox.Show(
                        "Registration failed.\n" +
                        "Please check your database and try again.",
                        "Registration Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to register account.\n\n" +
                    ex.Message +
                    "\n\nDatabase location:\n" +
                    DatabaseHelper.GetDatabasePath(),
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
