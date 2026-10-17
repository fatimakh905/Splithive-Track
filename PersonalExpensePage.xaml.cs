using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace SplitHive
{
    public partial class PersonalExpensePage : Page
    {
        private int userId;
        private string connectionString = "Data Source=Expense.db";

        public PersonalExpensePage(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            chkEnableBudget.Checked += (s, e) => txtBudget.IsEnabled = true;
            chkEnableBudget.Unchecked += (s, e) => txtBudget.IsEnabled = false;

            dpExpenseDate.SelectedDate = DateTime.Now;

            LoadBudget();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to previous page (e.g., HomePage)
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }


        private void LoadBudget()
        {
            string month = DateTime.Now.ToString("yyyy-MM");

            using var con = new SqliteConnection(connectionString);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT Amount FROM Budgets WHERE UserId=@uid AND Month=@month";
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@month", month);
            var result = cmd.ExecuteScalar();

            if (result != null)
            {
                txtBudget.Text = result.ToString();
                chkEnableBudget.IsChecked = true;
                txtBudget.IsEnabled = true;
            }
            else
            {
                txtBudget.Text = "";
                chkEnableBudget.IsChecked = false;
                txtBudget.IsEnabled = false;
            }
        }

        private void SaveBudget_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(txtBudget.Text, out double budget))
            {
                MessageBox.Show("Invalid budget amount");
                return;
            }

            string month = DateTime.Now.ToString("yyyy-MM");

            using var con = new SqliteConnection(connectionString);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"
                INSERT OR REPLACE INTO Budgets(UserId, Month, Amount)
                VALUES(@uid, @month, @amt)";
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@month", month);
            cmd.Parameters.AddWithValue("@amt", budget);
            cmd.ExecuteNonQuery();

            MessageBox.Show("Budget saved successfully!");
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(txtAmount.Text, out double amount))
            {
                MessageBox.Show("Enter valid amount");
                return;
            }

            string category = (cmbCategory.SelectedItem as ComboBoxItem)?.Content.ToString();
            string description = txtDescription.Text;
            string date = dpExpenseDate.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");

            using var con = new SqliteConnection(connectionString);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"INSERT INTO Expenses(UserId, Amount, Category, Date, Description)
                                VALUES(@uid, @amt, @cat, @date, @desc)";
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@amt", amount);
            cmd.Parameters.AddWithValue("@cat", category);
            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@desc", description);
            cmd.ExecuteNonQuery();

            MessageBox.Show("Expense added successfully!");
            LoadBudget(); // auto-refresh budget info
        }

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new ExpenseHistoryPage(userId));
        }
    }
}
