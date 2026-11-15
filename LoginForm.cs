
using System;
using System.Drawing;
using System.Windows.Forms;

namespace jasminelogin
{
    public partial class LoginForm : Form
    {
        private readonly TextBox txtUsername = new();
        private readonly TextBox txtPassword = new();

        public LoginForm()
        {
            Text = "Login";
            Size = new Size(450, 510);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(240, 250, 244);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Label title = new Label
            {
                Text = "LOGIN",
                Font = new Font("Segoe UI", 25, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 90, 50),
                Location = new Point(45, 45),
                Size = new Size(350, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label subtitle = new Label
            {
                Text = "Welcome back! Please log in.",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gray,
                Location = new Point(45, 100),
                Size = new Size(350, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            AddField("Username", txtUsername, 165);
            AddField("Password", txtPassword, 250);

            txtPassword.UseSystemPasswordChar = true;

            Button btnLogin = CreateButton("LOGIN", 105, 335);
            Button btnRegister = CreateButton(
                "CREATE ACCOUNT", 105, 390);

            btnLogin.Click += Login_Click;
            btnRegister.Click += Register_Click;

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(btnLogin);
            Controls.Add(btnRegister);
        }

        private void AddField(
            string labelText, TextBox textBox, int top)
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

        private Button CreateButton(string text, int x, int y)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(220, 42),
                BackColor = Color.FromArgb(35, 139, 82),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        private void Login_Click(object? sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your username and password.");
                return;
            }

            try
            {
                var user = DatabaseHelper.FindUser(username);

                if (user.HasValue &&
                    PasswordHasher.VerifyPassword(
                        password, user.Value.PasswordHash))
                {
                    Hide();

                    using (DashboardForm dashboard =
                        new DashboardForm(user.Value.FullName))
                    {
                        dashboard.ShowDialog();
                    }

                    txtPassword.Clear();
                    Show();
                }
                else
                {
                    MessageBox.Show(
                        "Incorrect username or password.",
                        "Login Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Login error:\n" + ex.Message,
                    "Login"
                );
            }
        }

        private void Register_Click(object? sender, EventArgs e)
        {
            using RegistrationForm registration =
                new RegistrationForm();

            registration.ShowDialog();
        }
    }
}
