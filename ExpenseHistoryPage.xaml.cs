using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SplitHive
{
    public partial class ExpenseHistoryPage : Page
    {
        private int userId;
        private string connectionString = "Data Source=Expense.db";

        public ExpenseHistoryPage(int userId)
        {
            InitializeComponent();
            this.userId = userId;

            // Default dates
            dpFromDate.SelectedDate = DateTime.Now.AddMonths(-1);
            dpToDate.SelectedDate = DateTime.Now;

            LoadExpenses();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void LoadExpenses(string fromDate = null, string toDate = null, string category = "All", string sortOrder = "None")
        {
            using var con = new SqliteConnection(connectionString);
            con.Open();

            var cmd = con.CreateCommand();
            string query = "SELECT Date, Category, Amount, Description FROM Expenses WHERE UserId=@uid";
            cmd.Parameters.AddWithValue("@uid", userId);

            if (!string.IsNullOrEmpty(fromDate))
            {
                query += " AND Date >= @from";
                cmd.Parameters.AddWithValue("@from", fromDate);
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                query += " AND Date <= @to";
                cmd.Parameters.AddWithValue("@to", toDate);
            }
            if (category != "All")
            {
                query += " AND Category=@cat";
                cmd.Parameters.AddWithValue("@cat", category);
            }

            if (sortOrder == "Low → High")
                query += " ORDER BY Amount ASC";
            else if (sortOrder == "High → Low")
                query += " ORDER BY Amount DESC";
            else
                query += " ORDER BY Date DESC";

            cmd.CommandText = query;

            using var reader = cmd.ExecuteReader();
            var dt = new DataTable();
            dt.Load(reader);
            dgExpenses.ItemsSource = dt.DefaultView;
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            if (dpFromDate.SelectedDate > dpToDate.SelectedDate && dpToDate.SelectedDate != null)
            {
                MessageBox.Show("'From Date' cannot be after 'To Date'", "Invalid Date", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string from = dpFromDate.SelectedDate?.ToString("yyyy-MM-dd");
            string to = dpToDate.SelectedDate?.ToString("yyyy-MM-dd");
            string category = (cmbFilterCategory.SelectedItem as ComboBoxItem)?.Content.ToString();
            string sortOrder = (cmbSortAmount.SelectedItem as ComboBoxItem)?.Content.ToString();

            LoadExpenses(from, to, category, sortOrder);
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (dgExpenses.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one expense to delete.");
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete selected expense(s)?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                using var con = new SqliteConnection(connectionString);
                con.Open();

                foreach (DataRowView row in dgExpenses.SelectedItems)
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandText = @"DELETE FROM Expenses 
                                        WHERE UserId=@uid AND Date=@date AND Amount=@amt AND Category=@cat AND Description=@desc";
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@date", row["Date"]);
                    cmd.Parameters.AddWithValue("@amt", row["Amount"]);
                    cmd.Parameters.AddWithValue("@cat", row["Category"]);
                    cmd.Parameters.AddWithValue("@desc", row["Description"]);
                    cmd.ExecuteNonQuery();
                }
                LoadExpenses();
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (dgExpenses.ItemsSource is not DataView view || view.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = "ExpenseHistory",
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (dlg.ShowDialog() != true) return;

            var sb = new StringBuilder();

            // ===== HEADERS (same order as DataTable) =====
            foreach (DataColumn col in view.Table.Columns)
                sb.Append(col.ColumnName + ",");
            sb.Length--;
            sb.AppendLine();

            // ===== ROWS =====
            foreach (DataRow row in view.Table.Rows)
            {
                foreach (DataColumn col in view.Table.Columns)
                {
                    // FIX DATE ISSUE
                    if (col.ColumnName.Equals("Date", StringComparison.OrdinalIgnoreCase) &&
                        DateTime.TryParse(row[col].ToString(), out DateTime dt))
                    {
                        sb.Append(dt.ToString("yyyy-MM-dd") + ",");
                    }
                    else
                    {
                        sb.Append(row[col].ToString().Replace(",", ";") + ",");
                    }
                }
                sb.Length--;
                sb.AppendLine();
            }

            File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);

            MessageBox.Show("Export successful!", "Export",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}
