using Banking_System.Data;
using Banking_System.Core.Input;
using Banking_System.Core.Logging;
using Banking_System.UserInterface.Menu;
using System.Text.RegularExpressions;
using SystemFile = System.IO.File;

namespace Banking_System.Core.File;

public static class FileManager
{
    private static readonly DataContext Context = new();

    public static void HandleFileManagement()
    {
        var exit = false;
        while (!exit)
        {
            AdminMenu.FileManagementMenu();
            switch (Console.ReadLine())
            {
                case "1":
                    ViewSystemLog();
                    break;
                case "2":
                    GetAccountStatement();
                    break;
                case "3":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Invalid choice, please try again.");
                    break;
            }

            if (!exit) MainMenuManager.KeyPressToContinue();
        }
    }
    
    public static void GetAccountStatement()
    {
        Console.WriteLine("Listing all accounts:");
        Context.Accounts
            .Select((account, index) => new { Account = account, Index = index }).ToList()
            .ForEach(item => Console.WriteLine(
                $"{item.Index + 1}. Account Number: {item.Account.AccountNumber}; " +
                $"Balance: {item.Account.Balance} {item.Account.Currency}; " +
                $"Type: {item.Account.AccountType}, " +
                $"Status: {(item.Account.IsActive ? "Active" : "Inactive")}"
            ));
        
        var accountNumber = InputHandler.PromptInput("Please enter the 16-digit account number to view the statement: ");
        if (!Regex.IsMatch(accountNumber, @"^\d{16}$"))
        {
            Console.WriteLine("The account number you entered is invalid. Ensure it is a 16-digit number.");
            return;
        }
    
        var statementFilePath = Path.Combine("Logs", "Accounts", $"account_{accountNumber}_statement.txt");
        if (!SystemFile.Exists(statementFilePath))
        {
            Console.WriteLine($"No statement found for account number {accountNumber}. Please make sure it exists.");
            return;
        }
    
        try
        {
            Console.WriteLine($"Displaying statement for account {accountNumber}:\n");
            SystemFile.ReadAllLines(statementFilePath).ToList().ForEach(Console.WriteLine);
            LoggerManager.LogSystemEvent("info", $"Statement for account {accountNumber} displayed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while fetching the account statement: {ex.Message}");
            LoggerManager.LogSystemEvent("error", $"Error while fetching the account statement: {ex.Message}");
        }
    }
    
    public static void ViewSystemLog()
    {
        var logDate = InputHandler.PromptDateTime("Enter the date of the system log file (yyyy-MM-dd): ", "yyyy-MM-dd");
        var logFilePath = Path.Combine("Logs", "System", $"bank_system_log{logDate:yyyyMMdd}.txt");
        if (!SystemFile.Exists(logFilePath))
        {
            Console.WriteLine($"No Log File Found for {logDate:yyyy-MM-dd}.");
            return;
        }
        
        if (logDate == DateTime.Today)
        {
            Console.WriteLine("The system log file cannot be accessed yet.");
            return;
        }
        
        try
        {
            Console.WriteLine($"Contents of the log file for {logDate:yyyy-MM-dd}:\n");
            SystemFile.ReadAllLines(logFilePath).ToList().ForEach(Console.WriteLine);
            LoggerManager.LogSystemEvent("info", $"System log for {logDate:yyyy-MM-dd} displayed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while reading the log file: {ex}");
            LoggerManager.LogSystemEvent("error", $"An error occurred while reading the log file: {ex.Message}");
        }
    }
}