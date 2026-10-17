using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SplitHive
{
    public partial class GroupListPage : Page
    {
        private int userId;
        private string connectionString = "Data Source=Expense.db";

        public GroupListPage(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadGroups();
        }

        private void LoadGroups()
        {
            using var con = new SqliteConnection(connectionString);
            con.Open();

            // Get groups
            var cmd = con.CreateCommand();
            cmd.CommandText = @"
                SELECT g.GroupId, g.GroupName, COALESCE(g.Description,'') AS Description
                FROM Groups g
                WHERE g.CreatedBy=@uid";
            cmd.Parameters.AddWithValue("@uid", userId);

            using var reader = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);

            // Add members as comma-separated string
            dt.Columns.Add("Members", typeof(string));
            foreach (DataRow row in dt.Rows)
            {
                int groupId = Convert.ToInt32(row["GroupId"]);
                var cmdMembers = con.CreateCommand();
                cmdMembers.CommandText = "SELECT Name FROM GroupMembers WHERE GroupId=@gid";
                cmdMembers.Parameters.AddWithValue("@gid", groupId);

                using var memberReader = cmdMembers.ExecuteReader();
                var sb = new StringBuilder();
                while (memberReader.Read())
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(memberReader.GetString(0));
                }
                row["Members"] = sb.ToString();
            }

            dgGroups.ItemsSource = dt.DefaultView;
        }

        private void ShowAddGroupPanel_Click(object sender, RoutedEventArgs e)
        {
            AddGroupPanel.Visibility = Visibility.Visible;
        }

        private void HideAddGroupPanel_Click(object sender, RoutedEventArgs e)
        {
            AddGroupPanel.Visibility = Visibility.Collapsed;
        }

        private void dgGroups_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgGroups.SelectedItem is DataRowView row)
            {
                int groupId = Convert.ToInt32(row["GroupId"]);
                string groupName = row["GroupName"].ToString();

                var settlePage = new GroupSettlePage(groupId, groupName);
                NavigationService.Navigate(settlePage);
            }
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            string groupName = txtNewGroupName.Text.Trim();
            string desc = txtNewGroupDesc.Text.Trim();
            string members = txtNewGroupMembers.Text.Trim();

            if (string.IsNullOrEmpty(groupName))
            {
                MessageBox.Show("Group name is required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var memberList = members.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(m => m.Trim()).Where(m => m != "").ToList();

            if (memberList.Count < 2)
            {
                MessageBox.Show("At least 2 member required", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var con = new SqliteConnection(connectionString);
            con.Open();

            var cmd = con.CreateCommand();
            cmd.CommandText = "INSERT INTO Groups (GroupName, Description, CreatedBy) VALUES (@name,@desc,@uid); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@name", groupName);
            cmd.Parameters.AddWithValue("@desc", desc);
            cmd.Parameters.AddWithValue("@uid", userId);
            long groupId = (long)cmd.ExecuteScalar();

            foreach (var member in memberList)
            {
                var cmdMember = con.CreateCommand();
                cmdMember.CommandText = "INSERT INTO GroupMembers (GroupId, Name) VALUES (@gid,@name)";
                cmdMember.Parameters.AddWithValue("@gid", groupId);
                cmdMember.Parameters.AddWithValue("@name", member);
                cmdMember.ExecuteNonQuery();
            }

            MessageBox.Show("Group created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            AddGroupPanel.Visibility = Visibility.Collapsed;
            txtNewGroupName.Clear();
            txtNewGroupDesc.Clear();
            txtNewGroupMembers.Clear();

            LoadGroups();
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (dgGroups.SelectedItem is DataRowView row)
            {
                int groupId = Convert.ToInt32(row["GroupId"]);
                string groupName = row["GroupName"].ToString();

                var result = MessageBox.Show(
                    $"Are you sure you want to delete the group '{groupName}' and all its members?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    using var con = new SqliteConnection(connectionString);
                    con.Open();
                    using var tran = con.BeginTransaction();

                    try
                    {
                        var cmdMembers = con.CreateCommand();
                        cmdMembers.CommandText = "DELETE FROM GroupMembers WHERE GroupId=@gid";
                        cmdMembers.Parameters.AddWithValue("@gid", groupId);
                        cmdMembers.ExecuteNonQuery();

                        var cmdGroup = con.CreateCommand();
                        cmdGroup.CommandText = "DELETE FROM Groups WHERE GroupId=@gid";
                        cmdGroup.Parameters.AddWithValue("@gid", groupId);
                        cmdGroup.ExecuteNonQuery();

                        tran.Commit();

                        MessageBox.Show($"Group '{groupName}' deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadGroups();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        MessageBox.Show($"Error deleting group: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a group to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}
