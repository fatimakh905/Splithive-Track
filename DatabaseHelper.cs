using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SplitHive
{
    public static class DatabaseHelper
    {
        // SQLite database file path
        public static void InitializeDatabase()
        {
            try
            {
                using var connection = new SqliteConnection("Data Source=Expense.db");
                connection.Open();

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Users(
                UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT,
                Email TEXT UNIQUE,
                Password TEXT
            );

            CREATE TABLE IF NOT EXISTS Expenses(
                ExpenseId INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER,
                Amount REAL,
                Category TEXT,
                Date TEXT,
                Description TEXT
            );

            CREATE TABLE IF NOT EXISTS Budgets(
                UserId INTEGER,
                Month TEXT,
                Amount REAL,
                PRIMARY KEY(UserId, Month),
                FOREIGN KEY(UserId) REFERENCES Users(UserId)
            );

            CREATE TABLE IF NOT EXISTS Groups(
                GroupId INTEGER PRIMARY KEY AUTOINCREMENT,
                GroupName TEXT,
                CreatedBy INTEGER
            );

            CREATE TABLE IF NOT EXISTS GroupMembers(
                MemberId INTEGER PRIMARY KEY AUTOINCREMENT,
                GroupId INTEGER,
                Name TEXT
            );

            CREATE TABLE IF NOT EXISTS GroupExpenses(
                GroupExpenseId INTEGER PRIMARY KEY AUTOINCREMENT,
                GroupId INTEGER,
                Amount REAL,
                PaidBy TEXT,
                Description TEXT,
                Date TEXT
            );

            CREATE TABLE IF NOT EXISTS GroupSettlements(
                SettlementId INTEGER PRIMARY KEY AUTOINCREMENT,
                GroupId INTEGER,
                FromMember TEXT,
                ToMember TEXT,
                Amount REAL,
                IsSettled INTEGER DEFAULT 0
            );
            
             CREATE TABLE IF NOT EXISTS GroupExpenseParticipants(
                 ParticipantId INTEGER PRIMARY KEY AUTOINCREMENT,
                  GroupExpenseId INTEGER,
                  MemberName TEXT
                  );
              

        ";
                tableCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

    }
}
// ALTER TABLE Groups ADD COLUMN Description TEXT; 
// ALTER TABLE GroupExpenses ADD COLUMN Category TEXT; 