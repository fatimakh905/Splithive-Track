using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SplitHive
{
    public partial class GroupSettlePage : Page
    {
        private readonly int groupId;
        private readonly string groupName;
        private const string ConnStr = "Data Source=Expense.db";

        public GroupSettlePage(int groupId, string groupName)
        {
            InitializeComponent();
            this.groupId = groupId;
            this.groupName = groupName;
            txtGroupTitleSettle.Text = this.groupName;

            LoadMembersSettle();
            LoadSettlementsSettle();
            LoadExpensesSettle();
        }

        private SqliteConnection GetConn() => new SqliteConnection(ConnStr);

        // ================= MEMBERS =================
        private void LoadMembersSettle()
        {
            cmbPaidBySettle.Items.Clear();
            spParticipantsSettle.Children.Clear();

            using var con = new SqliteConnection(ConnStr);
            con.Open();
            var cmd = new SqliteCommand("SELECT Name FROM GroupMembers WHERE GroupId=@g", con);
            cmd.Parameters.AddWithValue("@g", groupId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                string name = r.GetString(0);

                // PaidBy ComboBox
                cmbPaidBySettle.Items.Add(name);

                // Participants CheckBox
                var cb = new CheckBox
                {
                    Content = name,
                    Margin = new Thickness(5),
                    Foreground = System.Windows.Media.Brushes.Black
                };
                spParticipantsSettle.Children.Add(cb);
            }

            if (cmbPaidBySettle.Items.Count > 0)
                cmbPaidBySettle.SelectedIndex = 0;
        }


        // ================= ADD EXPENSE =================
        private void ShowAddExpensePanelSettle_Click(object sender, RoutedEventArgs e)
            => AddExpensePanelSettle.Visibility = Visibility.Visible;

        private void HideAddExpensePanelSettle_Click(object sender, RoutedEventArgs e)
            => AddExpensePanelSettle.Visibility = Visibility.Collapsed;

        private void AddExpenseSettle_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(txtExpenseAmountSettle.Text, out double amount) || amount <= 0)
            {
                MessageBox.Show("Invalid amount");
                return;
            }

            if (cmbPaidBySettle.SelectedItem == null)
            {
                MessageBox.Show("Select who paid");
                return;
            }

            if (cmbExpenseCategorySettle.SelectedItem == null)
            {
                MessageBox.Show("Select a category");
                return;
            }

            var participants = spParticipantsSettle.Children.OfType<CheckBox>()
                                .Where(c => c.IsChecked == true)
                                .Select(c => c.Content.ToString())
                                .ToList();

            if (participants.Count == 0)
            {
                MessageBox.Show("Select at least one participant");
                return;
            }

            string paidBy = cmbPaidBySettle.SelectedItem.ToString();
            string category = (cmbExpenseCategorySettle.SelectedItem as ComboBoxItem).Content.ToString();
            string desc = txtExpenseDescSettle.Text;
            string date = (dpExpenseDateSettle.SelectedDate ?? DateTime.Now).ToString("yyyy-MM-dd");

            using var con = new SqliteConnection(ConnStr);
            con.Open();
            using var tx = con.BeginTransaction();

            // Insert expense
            var cmdExp = new SqliteCommand(
                @"INSERT INTO GroupExpenses(GroupId,Amount,PaidBy,Description,Date,Category)
          VALUES(@g,@a,@p,@d,@dt,@cat)", con, tx);
            cmdExp.Parameters.AddWithValue("@g", groupId);
            cmdExp.Parameters.AddWithValue("@a", amount);
            cmdExp.Parameters.AddWithValue("@p", paidBy);
            cmdExp.Parameters.AddWithValue("@d", desc);
            cmdExp.Parameters.AddWithValue("@dt", date);
            cmdExp.Parameters.AddWithValue("@cat", category);
            cmdExp.ExecuteNonQuery();

            long expenseId = (long)new SqliteCommand("SELECT last_insert_rowid()", con, tx).ExecuteScalar();

            // Participants
            foreach (var m in participants)
            {
                var cp = new SqliteCommand(
                    @"INSERT INTO GroupExpenseParticipants(GroupExpenseId,MemberName)
              VALUES(@e,@m)", con, tx);
                cp.Parameters.AddWithValue("@e", expenseId);
                cp.Parameters.AddWithValue("@m", m);
                cp.ExecuteNonQuery();
            }

            // Update settlements
            double share = amount / participants.Count;
            foreach (var m in participants)
                if (m != paidBy)
                    UpsertSettlementSettle(con, tx, m, paidBy, share);

            tx.Commit();

            // Clear panel
            txtExpenseAmountSettle.Clear();
            txtExpenseDescSettle.Clear();
            dpExpenseDateSettle.SelectedDate = null;
            foreach (CheckBox cb in spParticipantsSettle.Children) cb.IsChecked = false;
            cmbExpenseCategorySettle.SelectedIndex = -1;

            HideAddExpensePanelSettle_Click(null, null);
            LoadSettlementsSettle();
            LoadExpensesSettle();
        }


        private void UpsertSettlementSettle(SqliteConnection con, SqliteTransaction tx, string from, string to, double amount)
        {
            var chk = new SqliteCommand(
                @"SELECT SettlementId,Amount FROM GroupSettlements
                  WHERE GroupId=@g AND FromMember=@f AND ToMember=@t AND IsSettled=0", con, tx);
            chk.Parameters.AddWithValue("@g", groupId);
            chk.Parameters.AddWithValue("@f", from);
            chk.Parameters.AddWithValue("@t", to);

            using var r = chk.ExecuteReader();
            if (r.Read())
            {
                int id = r.GetInt32(0);
                double oldAmt = r.GetDouble(1);
                r.Close();

                var upd = new SqliteCommand(
                    "UPDATE GroupSettlements SET Amount=@a WHERE SettlementId=@id", con, tx);
                upd.Parameters.AddWithValue("@a", oldAmt + amount);
                upd.Parameters.AddWithValue("@id", id);
                upd.ExecuteNonQuery();
            }
            else
            {
                r.Close();
                var ins = new SqliteCommand(
                    @"INSERT INTO GroupSettlements(GroupId,FromMember,ToMember,Amount)
                      VALUES(@g,@f,@t,@a)", con, tx);
                ins.Parameters.AddWithValue("@g", groupId);
                ins.Parameters.AddWithValue("@f", from);
                ins.Parameters.AddWithValue("@t", to);
                ins.Parameters.AddWithValue("@a", amount);
                ins.ExecuteNonQuery();
            }
        }

        // ================= LOAD SETTLEMENTS =================
        private void LoadSettlementsSettle()
        {
            var list = new List<SettlementRow2>();

            using var con = GetConn();
            con.Open();

            var cmd = new SqliteCommand(
                @"SELECT SettlementId, FromMember, ToMember, Amount
          FROM GroupSettlements
          WHERE GroupId=@g AND IsSettled=0", con);

            cmd.Parameters.AddWithValue("@g", groupId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new SettlementRow2
                {
                    SettlementId = r.GetInt32(0),
                    FromMember = r.GetString(1),
                    ToMember = r.GetString(2),
                    Amount = r.GetDouble(3)
                });
            }

            dgSettlementsSettle.ItemsSource = list;
        }


        private void SettleSettle_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            using var con = GetConn();
            con.Open();

            var cmd = new SqliteCommand(
                "UPDATE GroupSettlements SET IsSettled=1 WHERE SettlementId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            LoadSettlementsSettle();
        }

        // ================= LOAD EXPENSES =================
        private void LoadExpensesSettle()
        {
            using var con = GetConn();
            con.Open();

            var dt = new DataTable();
            var cmd = new SqliteCommand(
                @"SELECT 
            GroupExpenseId,
            Date,
            Category,
            Amount,
            PaidBy,
            Description
          FROM GroupExpenses
          WHERE GroupId=@g
          ORDER BY Date DESC", con);

            cmd.Parameters.AddWithValue("@g", groupId);
            dt.Load(cmd.ExecuteReader());

            dgExpensesSettle.ItemsSource = dt.DefaultView;
        }



        private void ApplyFilterSettle_Click(object sender, RoutedEventArgs e)
        {
            using var con = GetConn();
            con.Open();

            string query = "SELECT GroupExpenseId, Date, Category, Amount, PaidBy, Description FROM GroupExpenses WHERE GroupId=@g";
            var filters = new List<string>();
            var cmd = new SqliteCommand();
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@g", groupId);

            if (dpFromDateSettle.SelectedDate.HasValue)
                filters.Add("Date >= @from");
            if (dpToDateSettle.SelectedDate.HasValue)
                filters.Add("Date <= @to");
            if (cmbFilterCategorySettle.SelectedItem is ComboBoxItem item && item.Content.ToString() != "All")
                filters.Add("Category = @cat");

            if (filters.Count > 0)
                query += " AND " + string.Join(" AND ", filters);

            cmd.CommandText = query;

            if (dpFromDateSettle.SelectedDate.HasValue)
                cmd.Parameters.AddWithValue("@from", dpFromDateSettle.SelectedDate.Value.ToString("yyyy-MM-dd"));
            if (dpToDateSettle.SelectedDate.HasValue)
                cmd.Parameters.AddWithValue("@to", dpToDateSettle.SelectedDate.Value.ToString("yyyy-MM-dd"));
            if (cmbFilterCategorySettle.SelectedItem is ComboBoxItem item2 && item2.Content.ToString() != "All")
                cmd.Parameters.AddWithValue("@cat", item2.Content.ToString());

            var dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            dgExpensesSettle.ItemsSource = dt.DefaultView;
        }

        // ================= DELETE EXPENSE =================
        private void DeleteSelectedSettle_Click(object sender, RoutedEventArgs e)
        {
            if (dgExpensesSettle.SelectedItem is DataRowView row)
            {
                long id = (long)row["GroupExpenseId"];
                using var con = GetConn();
                con.Open();

                var tx = con.BeginTransaction();

                var delPart = new SqliteCommand("DELETE FROM GroupExpenseParticipants WHERE GroupExpenseId=@id", con, tx);
                delPart.Parameters.AddWithValue("@id", id);
                delPart.ExecuteNonQuery();

                var delExp = new SqliteCommand("DELETE FROM GroupExpenses WHERE GroupExpenseId=@id", con, tx);
                delExp.Parameters.AddWithValue("@id", id);
                delExp.ExecuteNonQuery();

                tx.Commit();
                LoadExpensesSettle();
            }
        }

        // ================= EXPORT =================
        private void ExportExpensesSettle_Click(object sender, RoutedEventArgs e)
        {
            if (dgExpensesSettle.ItemsSource is not DataView view || view.Count == 0)
            {
                MessageBox.Show("No expenses to export.", "Export",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = $"{groupName}_Expenses.csv",
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (dlg.ShowDialog() != true) return;

            var sb = new StringBuilder();

            // ===== GROUP INFO =====
            sb.AppendLine($"Group Name,{groupName}");
            sb.AppendLine($"Group ID,{groupId}");
            sb.AppendLine($"Export Date,{DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();

            // ===== HEADERS =====
            foreach (DataColumn col in view.Table.Columns)
                sb.Append(col.ColumnName + ",");
            sb.Length--;
            sb.AppendLine();

            // ===== DATA =====
            foreach (DataRow row in view.Table.Rows)
            {
                foreach (DataColumn col in view.Table.Columns)
                {
                    if (col.ColumnName == "Date" &&
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

            MessageBox.Show("Expenses exported successfully!", "Export",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void ExportSettlementsSettle_Click(object sender, RoutedEventArgs e)
        {
            if (dgSettlementsSettle.ItemsSource is not List<SettlementRow2> list || list.Count == 0)
            {
                MessageBox.Show("No settlements to export.", "Export",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog dlg = new SaveFileDialog
            {
                FileName = $"{groupName}_Settlements.csv",
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (dlg.ShowDialog() != true) return;

            var sb = new StringBuilder();

            // ===== GROUP INFO =====
            sb.AppendLine($"Group Name,{groupName}");
            sb.AppendLine($"Group ID,{groupId}");
            sb.AppendLine($"Export Date,{DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();

            // ===== HEADERS =====
            sb.AppendLine("From,To,Amount");

            // ===== DATA =====
            foreach (var s in list)
            {
                sb.AppendLine($"{s.FromMember},{s.ToMember},{s.Amount}");
            }

            File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);

            MessageBox.Show("Settlements exported successfully!", "Export",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }



        // ================= BACK =================
        private void BackSettle_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }

    public class SettlementRow2
    {
        public int SettlementId { get; set; }
        public string FromMember { get; set; }
        public string ToMember { get; set; }
        public double Amount { get; set; }
    }
}
