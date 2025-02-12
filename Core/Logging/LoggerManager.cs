using Serilog;
using Serilog.Core;
using Banking_System.Data;
using Banking_System.Models;
using BankAccount = Banking_System.Models.Account;

namespace Banking_System.Core.Logging;

public static class LoggerManager
{
    private const string LogFileDirectory = "Logs";
    private const string SystemLogFileDirectory = "System";
    private const string AccountsLogFileDirectory = "Accounts";
    
    private static readonly DataContext DbContext = new();

    private static readonly string SystemLogFullPath = Path.Combine(LogFileDirectory, SystemLogFileDirectory, "bank_system_log.txt");
    private static readonly string AccountLogDirectoryFullPath = Path.Combine(LogFileDirectory, AccountsLogFileDirectory);

    private static readonly Logger SystemLogger = new LoggerConfiguration()
        .WriteTo.File(SystemLogFullPath, rollingInterval: RollingInterval.Day)
        .WriteTo.Console()
        .CreateLogger();
    
    static LoggerManager()
    {
        Directory.CreateDirectory(LogFileDirectory);
        Directory.CreateDirectory(Path.Combine(LogFileDirectory, SystemLogFileDirectory));
        Directory.CreateDirectory(AccountLogDirectoryFullPath);
    }

    public static void LogSystemEvent(string level, string message)
    {
        LogEvent(SystemLogger, level, message);
    }

    public static void LogAccountStatement(Client client, BankAccount account)
    {
        account.Transactions = DbContext.Transactions.Where(t => t.AccountId == account.Id).ToList();
        var accountFile = Path.Combine(AccountLogDirectoryFullPath, $"account_{account.AccountNumber}_statement.txt");
        
        using var writer = new StreamWriter(accountFile);
        writer.WriteLine($"Account Holder: {client.FirstName} {client.LastName}");
        writer.WriteLine($"Account Number: {account.AccountNumber}");
        writer.WriteLine($"Account Type: {account.AccountType}");
        writer.WriteLine($"Open Date: {account.OpenDate}");
        writer.WriteLine($"Current Balance: {account.Balance} {account.Currency}");
        writer.WriteLine($"Is Active: {account.IsActive}");

        if (account.Transactions is { Count: > 0 }) LogTransactions(writer, account.Transactions);
        else writer.WriteLine("Transactions: None");
        Console.WriteLine($"Statement for Account {account.AccountNumber} Created Successfully");
    }

    private static void LogTransactions(StreamWriter writer, List<Transaction> transactions)
    {
        writer.WriteLine("Transactions: ");
        foreach (var transaction in transactions)
        {
            writer.Write($" -- Amount: {transaction.Amount}, ");
            writer.Write($"Transaction Type: {transaction.TransactionType}, ");
            writer.Write($"Transaction Date: {transaction.TransactionDate}, ");
            writer.Write($"Description: {transaction.Description}, ");
            writer.Write($"Status: {transaction.Status}\n");
        }
    }
    
    private static void LogEvent(Logger logger, string level, string message)
    {
        switch (level.ToLower())
        {
            case "debug":
                logger.Debug(message);
                break;
            case "info":
                logger.Information(message);
                break;
            case "warning":
                logger.Warning(message);
                break;
            case "error":
                logger.Error(message);
                break;
            case "fatal":
                logger.Fatal(message);
                break;
            default:
                logger.Information($"Unknown log level '{level}'. Defaulting to Information. Message: {message}");
                break;
        }
    }
}
