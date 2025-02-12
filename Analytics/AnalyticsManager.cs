using Banking_System.Data;
using Banking_System.Core.Input;
using Banking_System.UserInterface.Menu;
using Banking_System.Communication.Exchange;
using Microsoft.EntityFrameworkCore;

namespace Banking_System.Analytics;

public static class AnalyticsManager
{
    private static readonly DataContext Context = new();
    
    public static void HandleAnalyticsManagement()
    {
        var exit = false;
        while (!exit)
        {
            AdminMenu.AnalyticsManagementMenu();
            switch (Console.ReadLine())
            {
                case "1":
                    var minAccounts = InputHandler.PromptIntInput("Enter the minimum number of accounts: ");
                    GetClientsWithMultipleAccounts(minAccounts);
                    break;
                case "2":
                    var startDate = InputHandler.PromptDateTime("Enter the start date (yyyy-mm-dd): ", "yyyy-mm-dd");
                    var endDate = InputHandler.PromptDateTime("Enter the end date (yyyy-mm-dd): ", "yyyy-mm-dd");
                    GetTransactionsInDateRange(startDate, endDate);
                    break;
                case "3":
                    GetMostActiveClient();
                    break;
                case "4":
                    GetLargestTransactions(5); // Top 5 largest transactions
                    break;
                case "5":
                    GetTransactionCountByStatus();
                    break;
                case "6":
                    var months = InputHandler.PromptIntInput("Enter the number of months: ");
                    GetRecentlyOpenedAccounts(months);
                    break;
                case "7":
                    var topNActiveAccounts = InputHandler.PromptIntInput("Enter the number of most active accounts to display: ");
                    GetMostActiveAccounts(topNActiveAccounts);
                    break;
                case "8":
                    var topCustomers = InputHandler.PromptIntInput("Enter the number of top customers by balance to display: ");
                    GetClientsWithHighestBalance(topCustomers);
                    break;
                case "9":
                    GetDailyTransactionReport();
                    break;
                case "10":
                    GetAccountStatisticsByCurrency();
                    break;
                case "11":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Invalid choice, please try again.");
                    break;
            }
            if (!exit) MainMenuManager.KeyPressToContinue();
        }
    }
    
    public static void GetClientsWithMultipleAccounts(int minAccounts)
    {
        // Fetch clients who have at least 'minAccounts' accounts
        var clients = Context.Clients
              .Where(c => c.Accounts != null && c.Accounts.Count >= minAccounts)
              .Include(client => client.Accounts!) // Include related accounts for each client
              .ToList();
        
        // Display the clients with their account count
        Console.WriteLine("Clients with multiple accounts:");
        clients.ForEach(c => Console.WriteLine($"{c.FirstName} {c.LastName}, Accounts: {c.Accounts!.Count}"));
    }

    public static void GetTransactionsInDateRange(DateTime startDate, DateTime endDate)
    {
        // Fetch transactions that occurred within the given date range
        var transactions = Context.Transactions
              .Include(t => t.Account) // Include account details for each transaction
              .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
              .ToList();

        // Display the transactions with date and amount
        Console.WriteLine($"Transactions from {startDate} to {endDate}:");
        transactions.ForEach(t => Console.WriteLine($"{t.TransactionDate}: {t.Amount} {t.Account!.Currency}"));
    }

    public static void GetMostActiveClient()
    {
        // Fetch all clients with their accounts and transactions
        var mostActiveClient = Context.Clients
            .Include(c => c.Accounts)!
            .ThenInclude(a => a.Transactions)
            .ToList()  // Load all clients with accounts and transactions into memory
            .OrderByDescending(c => c.Accounts!.Sum(a => a.Transactions!.Count))  // Perform aggregation locally in memory
            .FirstOrDefault(); // Get the most active client

        // Display the most active client
        Console.WriteLine(mostActiveClient != null 
            ? $"Most active client: {mostActiveClient.FirstName} {mostActiveClient.LastName}" 
            : "No clients found.");
    }

    public static void GetLargestTransactions(int topN)
    {
        // Fetch the top N transactions ordered by amount in descending order
        var transactions = Context.Transactions
              .Include(t => t.Account) // Include account details for each transaction
              .OrderByDescending(t => t.Amount) // Order by transaction amount
              .Take(topN) // Limit to the top N transactions
              .ToList();

        // Display the top N largest transactions
        Console.WriteLine($"Top {topN} largest transactions:");
        transactions.ForEach(t => Console.WriteLine($"{t.Amount:F2} {t.Account!.Currency}, Date: {t.TransactionDate}"));
    }

    public static void GetTransactionCountByStatus()
    {
        // Group transactions by their status and count each group
        var transactionCountByStatus = Context.Transactions
            .Include(t => t.Account) // Include account details for each transaction
            .GroupBy(t => t.Status) // Group by transaction status
            .ToDictionary(g => g.Key, g => g.Count()); // Create a dictionary of status and count

        // Display the transaction count by status
        Console.WriteLine("Transaction counts by status:");
        foreach (var kvp in transactionCountByStatus) 
            Console.WriteLine($"{kvp.Key}: {kvp.Value} transactions");
    }

    public static void GetRecentlyOpenedAccounts(int months)
    {
        // Calculate the cutoff date based on the number of months
        var cutoffDate = DateTime.Now.AddMonths(-months);
        
        // Fetch accounts opened after the cutoff date
        var accounts = Context.Accounts
            .Where(a => a.OpenDate >= cutoffDate)
            .ToList();

        // Display the accounts opened recently
        Console.WriteLine($"Accounts opened in the last {months} months:");
        accounts.ForEach(a => Console.WriteLine($"{a.AccountNumber}, Open Date: {a.OpenDate}"));
    }

    public static void GetMostActiveAccounts(int topN)
    {
        // Fetch the top N accounts ordered by transaction count in descending order
        var mostActiveAccounts = Context.Accounts
            .OrderByDescending(a => a.Transactions!.Count) // Order accounts by the number of transactions
            .Take(topN) // Limit to the top N accounts
            .Select(a => new { a.AccountNumber, TransactionCount = a.Transactions!.Count }) // Select account number and transaction count
            .AsEnumerable() // Perform in-memory processing for final selection
            .ToDictionary(a => a.AccountNumber!, a => a.TransactionCount); // Create a dictionary of account numbers and transaction counts

        // Display the top N most active accounts
        Console.WriteLine($"Top {topN} most active accounts:");
        foreach (var kvp in mostActiveAccounts) 
            Console.WriteLine($"{kvp.Key}: {kvp.Value} transactions");
    }

    public static void GetClientsWithHighestBalance(int topN)
    {
        // Get the exchange rates from the StaticExchangeRateManager
        var exchangeRates = StaticExchangeRateManager.GetExchangeRates();
        
        // Fetch the clients with the highest balance 
        var customersWithHighestBalance = Context.Clients
            .Select(c => new
            {
                FullName = $"{c.FirstName} {c.LastName}",
                Accounts = c.Accounts!
            })
            .AsEnumerable()  // Convert the result to in-memory collection
            .Select(c => new
            {
                c.FullName,
                // Sum the balances after converting the amounts to GEL
                TotalBalance = c.Accounts.Sum(a => a.Balance * exchangeRates!.GetValueOrDefault(a.Currency, 1m))
            })
            .OrderByDescending(c => c.TotalBalance)
            .Take(topN)
            .ToDictionary(c => c.FullName, c => c.TotalBalance);
        
        Console.WriteLine($"Top {topN} customers with highest balance:");
        foreach (var kvp in customersWithHighestBalance)
            Console.WriteLine($"{kvp.Key}: {kvp.Value:F2} GEL balance");
    }

    public static void GetDailyTransactionReport()
    {
        // Get the exchange rates from the StaticExchangeRateManager
        var exchangeRates = StaticExchangeRateManager.GetExchangeRates();

        // Perform the query to fetch the transactions and account details
        var dailyTransactionReport = Context.Transactions
            .Include(t => t.Account)  // Ensure account details are loaded
            .GroupBy(t => t.TransactionDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                TransactionCount = g.Count(),
                Transactions = g.ToList()  // Materialize the group into a list to handle conversion later
            })
            .AsEnumerable()  // Convert to in-memory collection for further processing
            .Select(g => new
            {
                g.Date,
                g.TransactionCount,
                // Sum the amounts after converting to GEL
                TotalAmount = g.Transactions.Sum(t => t.Amount * exchangeRates!.GetValueOrDefault(t.Account!.Currency, 1m))
            })
            .ToList();

        // Display the daily transaction report
        Console.WriteLine("Daily transaction report:");
        dailyTransactionReport.ForEach(r =>
            Console.WriteLine($"Date: {r.Date}, Transactions: {r.TransactionCount}, Total Amount: {r.TotalAmount:F2} GEL")
        );
    }

    public static void GetAccountStatisticsByCurrency()
    {
        // Fetch account statistics by currency
        var accountStatisticsByCurrency = Context.Accounts
            .GroupBy(a => a.Currency)
            .Select(g => new
            {
                Currency = g.Key!,
                AccountCount = g.Count(),
                TotalBalance = g.Sum(a => a.Balance),
                AverageBalance = g.Average(a => a.Balance)
            })
            .ToList();

        // Display the account statistics by currency
        Console.WriteLine("Account statistics by currency:");
        accountStatisticsByCurrency.ForEach(s =>
            Console.WriteLine($"{s.Currency}: {s.AccountCount} accounts, Total Balance: {s.TotalBalance:F2} {s.Currency}, Average Balance: {s.AverageBalance} {s.Currency}")
        );
    }
}
