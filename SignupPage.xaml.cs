using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SplitHive
{
    /// <summary>
    /// Interaction logic for SignupPage.xaml
    /// </summary>
    public partial class SignupPage : Page
    {
        public SignupPage()
        {
            InitializeComponent();
        }

        private void Signup_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill all fields", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Please enter a valid email", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 4 || password.Length > 8)
            {
                MessageBox.Show("Password must be between 4 and 8 characters", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var connection = new SqliteConnection("Data Source=Expense.db");
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT INTO Users (Name, Email, Password) VALUES ($name, $email, $password)";
                cmd.Parameters.AddWithValue("$name", name);
                cmd.Parameters.AddWithValue("$email", email);
                cmd.Parameters.AddWithValue("$password", password);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Signup successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                this.NavigationService.Navigate(new LoginPage());
            }
            catch (SqliteException ex)
            {
                MessageBox.Show("Database error: " + ex.Message);
            }
        }

        private void GoToLogin_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LoginPage());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }
    }
}
