 <div align="center">

# SplitHive Track
### Personal & Group Expense Tracker

A Windows desktop application for tracking personal spending, managing budgets, and organizing shared group expenses with settlement tracking.

[![C#](https://img.shields.io/badge/C%23-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?style=flat-square&logo=windows&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
[![SQLite](https://img.shields.io/badge/SQLite-003B57?style=flat-square&logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![Visual Studio](https://img.shields.io/badge/Visual%20Studio-5C2D91?style=flat-square&logo=visualstudio&logoColor=white)](https://visualstudio.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue?style=flat-square)](LICENSE)

</div>

## About the Project

**SplitHive Track** is a C# desktop application built with Windows Presentation Foundation (WPF) to simplify personal expense management and shared group spending. It provides a central dashboard for reviewing expenses, monitoring budget usage, viewing recent transactions, and tracking group settlements.

The application uses SQLite to store user information, personal expenses, monthly budgets, groups, shared expenses, and settlement records locally.

## Features

- **User Login and Registration:** Access the application through dedicated login and signup pages.
- **Expense Dashboard:** View total expenses, budget information, group counts, pending settlements, and recent transactions.
- **Personal Expense Tracking:** Record individual expenses with amounts, categories, dates, and descriptions.
- **Monthly Budget Management:** Set budgets and review spending against the configured budget.
- **Expense History:** Browse transactions with date-range filtering, category filtering, and sorting options.
- **Group Expense Management:** Create and manage groups for organizing shared expenses.
- **Expense Sharing:** Record group expenses, including the payer and participating members.
- **Settlement Tracking:** Track amounts owed between group members and mark settlements as completed.
- **Local Database:** Store application data in an SQLite database.

## Technologies Used

| Technology | Purpose |
|---|---|
| C# | Application logic and event handling |
| .NET 8 | Application runtime and framework |
| WPF | Windows desktop user interface |
| XAML | UI layout and styling |
| SQLite | Local data storage |
| Microsoft.Data.Sqlite | Database connectivity |
| Visual Studio | Development environment |

## Application Modules

| Module | Description |
|---|---|
| Login and Signup | User access and account registration |
| Dashboard | Expense summaries, budget progress, and recent activity |
| Personal Expenses | Individual spending and monthly budget management |
| Expense History | Transaction browsing and filtering |
| Group Management | Group creation and member management |
| Group Settlements | Shared expense settlement tracking |

## Getting Started

### Prerequisites

Before running the project, ensure you have:

- Windows
- Visual Studio with the **.NET desktop development** workload
- .NET 8 SDK
- Git, or the option to download the repository as a ZIP file

### Installation and Setup

**1. Clone the repository**

```bash
git clone https://github.com/fatimakh905/Spithive-Track.git
```

**2. Open the solution**

Open `SplitHive.sln` in Visual Studio.

**3. Restore dependencies**

Allow Visual Studio to restore the required NuGet packages. The project uses `Microsoft.Data.Sqlite`.

**4. Build the application**

Select **Build > Build Solution** in Visual Studio.

**5. Run the application**

Press `F5` to build and launch SplitHive.

The application initializes its SQLite database using `Expense.db`. Keep the database file in the expected working directory so the application can access its stored data.


## Future Enhancements

Potential improvements for future versions include:

- Expense charts and spending trend analysis
- Exporting expense history to CSV or PDF
- Improved password security
- Recurring expense tracking
- More detailed group balance summaries
- Database backup and restore options

## Connect With Me

<div align="center">

**Kaneez Fatima**

[GitHub](https://github.com/fatimakh905) · [LinkedIn](https://www.linkedin.com/in/kaneez-fatima-8a1516335)

</div>
