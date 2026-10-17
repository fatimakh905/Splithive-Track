using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace SplitHive
{
    public partial class DashboardPage : Page
    {
        private int userId;
        private string userName;
        private string connectionString = "Data Source=Expense.db";

        public DashboardPage(int userId, string userName)
        {
            InitializeComponent();
            this.userId = userId;
            this.userName = userName;

            UserGreeting.Text = $"Hello, {userName} 👋";

            this.Loaded += DashboardPage_Loaded;
        }

        private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            using var con = new SqliteConnection(connectionString);
            con.Open();

            LoadCards(con);
            LoadRecentExpenses(con);
            LoadBudgetProgress(con);
        }

        private void LoadCards(SqliteConnection con)
        {
            // Total Expenses
            var cmdTotal = con.CreateCommand();
            cmdTotal.CommandText = "SELECT IFNULL(SUM(Amount),0) FROM Expenses WHERE UserId=$uid";
            cmdTotal.Parameters.AddWithValue("$uid", userId);
            TotalExpensesCard.Text = "Rs " + cmdTotal.ExecuteScalar();

            // Remaining Budget
            var cmdBudget = con.CreateCommand();
            cmdBudget.CommandText = @"
                SELECT IFNULL(Amount,0)
                FROM Budgets
                WHERE UserId=$uid
                ORDER BY Month DESC LIMIT 1";
            cmdBudget.Parameters.AddWithValue("$uid", userId);
            double budget = Convert.ToDouble(cmdBudget.ExecuteScalar() ?? 0);

            var cmdSpent = con.CreateCommand();
            cmdSpent.CommandText = "SELECT IFNULL(SUM(Amount),0) FROM Expenses WHERE UserId=$uid";
            cmdSpent.Parameters.AddWithValue("$uid", userId);
            double spent = Convert.ToDouble(cmdSpent.ExecuteScalar() ?? 0);

            RemainingBudgetCard.Text = "Rs " + (budget - spent);

            // Total Groups
            var cmdGroups = con.CreateCommand();
            cmdGroups.CommandText = "SELECT COUNT(*) FROM Groups WHERE CreatedBy=$uid";
            cmdGroups.Parameters.AddWithValue("$uid", userId);
            TotalGroupsCard.Text = cmdGroups.ExecuteScalar().ToString();

            // Pending Settlements
            var cmdPending = con.CreateCommand();
            cmdPending.CommandText = @"
    SELECT COUNT(*)
    FROM GroupSettlements gs
    JOIN Groups g ON gs.GroupId = g.GroupId
    WHERE gs.IsSettled = 0
      AND g.CreatedBy = $uid
";
            cmdPending.Parameters.AddWithValue("$uid", userId);

            PendingSettlementsCard.Text = cmdPending.ExecuteScalar().ToString();

        }

        private void LoadRecentExpenses(SqliteConnection con)
        {
            var cmd = con.CreateCommand();
            cmd.CommandText = @"
                SELECT Date, Description, Category, Amount
                FROM Expenses
                WHERE UserId=$uid
                ORDER BY Date DESC
                LIMIT 5";
            cmd.Parameters.AddWithValue("$uid", userId);

            using var reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            RecentExpensesGrid.ItemsSource = dt.DefaultView;
        }

        private void LoadBudgetProgress(SqliteConnection con)
        {
            double budget = 0;
            double spent = 0;

            // Latest budget
            var cmdBudget = con.CreateCommand();
            cmdBudget.CommandText = @"
                SELECT IFNULL(Amount,0)
                FROM Budgets
                WHERE UserId=$uid
                ORDER BY Month DESC LIMIT 1";
            cmdBudget.Parameters.AddWithValue("$uid", userId);
            budget = Convert.ToDouble(cmdBudget.ExecuteScalar() ?? 0);

            // Total spent
            var cmdSpent = con.CreateCommand();
            cmdSpent.CommandText = "SELECT IFNULL(SUM(Amount),0) FROM Expenses WHERE UserId=$uid";
            cmdSpent.Parameters.AddWithValue("$uid", userId);
            spent = Convert.ToDouble(cmdSpent.ExecuteScalar() ?? 0);

            double progress = budget == 0 ? 0 : Math.Min(spent / budget, 1);
            BudgetProgress.Value = progress * 100;
            BudgetText.Text = $"Spent Rs {spent} of Rs {budget}";
        }

        // Quick Actions
        private void AddPersonalExpense_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PersonalExpensePage(userId));
        }

        private void ViewGroups_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GroupListPage(userId));
        }

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ExpenseHistoryPage(userId));
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                NavigationService.Navigate(new LoginPage());
            }
        }

    
    }
}
