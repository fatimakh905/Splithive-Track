using Microsoft.Data.Sqlite;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SplitHive
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Please fill all fields",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show(
                    "Please enter a valid email",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (password.Length < 4 || password.Length > 8)
            {
                MessageBox.Show(
                    "Password must be between 4 and 8 characters",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            try
            {
                using var connection = new SqliteConnection("Data Source=Expense.db");
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText =
                    "SELECT UserId, Name FROM Users " +
                    "WHERE Email = $email AND Password = $password";

                cmd.Parameters.AddWithValue("$email", email);
                cmd.Parameters.AddWithValue("$password", password);

                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int userId = reader.GetInt32(0);
                    string name = reader.GetString(1);

                    this.NavigationService.Navigate(
                        new DashboardPage(userId, name)
                    );
                }
                else
                {
                    MessageBox.Show(
                        "Invalid email or password",
                        "Login Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
            catch (SqliteException ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            }
        }

        private void GoToSignup_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SignupPage());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }
    }
}
