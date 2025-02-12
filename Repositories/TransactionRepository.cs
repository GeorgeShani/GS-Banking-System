using Banking_System.Data;
using Banking_System.Enums;
using Banking_System.Interfaces;
using Banking_System.Models;
using Banking_System.Core.Input;
using Banking_System.Core.Logging;
using Banking_System.Security.Validation;
using Banking_System.UserInterface.Menu;
using Microsoft.EntityFrameworkCore;

namespace Banking_System.Repositories;

public class TransactionRepository(Client systemUser, DataContext context) : ITransactionRepository
{
    public void HandleTransactionManagement()
    {
        var exit = false;
        while (!exit)
        {
            AdminMenu.TransactionManagementMenu();
            switch (Console.ReadLine())
            {
                case "1":
                    CreateTransaction();
                    break;
                case "2":
                    ReadAllTransactions();
                    break;
                case "3":
                    UpdateTransaction();
                    break;
                case "4":
                    DeleteTransaction();
                    break;
                case "5":
                    exit = true;
                    break;
            }
            
            if (!exit) MainMenuManager.KeyPressToContinue();
        }
    }
    
    public void CreateTransaction()
    {
        if (!ValidateAdminAccess()) return;
        
        var accountNumber = InputHandler.PromptInput("Enter account number: ");
        var account = context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefault(a => a.AccountNumber == accountNumber);
        
        var transaction = new Transaction
        {
            Amount = InputHandler.PromptDecimalInput("Enter transaction amount: "),
            TransactionType = InputHandler.PromptEnum<TransactionType>("Enter transaction type: "),
            Description = InputHandler.PromptInput("Enter transaction description: "),
            Status = InputHandler.PromptEnum<Status>("Enter transaction status: "),
            TransactionDate = DateTime.Now,
            AccountId = account!.Id,
            Account = account
        };

        if (!ValidateTransaction(transaction)) return;

        try
        {
            context.Transactions.Add(transaction);
            context.SaveChanges();
            LoggerManager.LogSystemEvent("info", $"New Transaction Added. Amount: {transaction.Amount} {account.Currency}, Transaction Type: {transaction.TransactionType}, TransactionDate: {transaction.TransactionDate}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error adding new transaction: " + ex);
            LoggerManager.LogSystemEvent("error", $"Failed to add a new transaction: {ex.Message}");   
        }
    }

    // Reads system user's transactions
    public void ReadTransactions()
    {
        var systemUserTransactions =context.Transactions
            .Include(t => t.Account)
            .ThenInclude(a => a!.Client)
            .Where(t => t.Account!.ClientId == systemUser.Id)
            .ToList();

        Console.WriteLine("Displaying all transactions associated with your accounts...");
        PrintTransactions(systemUserTransactions);
    }
    
    // Gives access to all transactions in the system to the admin
    public void ReadAllTransactions()
    {
        if (!ValidateAdminAccess()) return;
        var transactions = context.Transactions
            .Include(t => t.Account)
            .ToList();
        
        Console.WriteLine("Displaying all transactions in the system...");
        PrintTransactions(transactions, true);
    }

    public void UpdateTransaction()
    {
        if (!ValidateAdminAccess()) return;
        if (!context.Transactions.Any())
        {
            Console.WriteLine("No transactions in the database.");
            return;
        }
            
        ReadAllTransactions();

        var transactionId = InputHandler.PromptIntInput("Enter the ID of the transaction to update: ");
        var transaction = context.Transactions.FirstOrDefault(t => t.Id == transactionId);
        if (transaction == null)
        {
            Console.WriteLine("Transaction not found.");
            return;
        }

        var fieldChoices = new Dictionary<string, Action>
        {
            { "1", () => transaction.Amount = InputHandler.PromptDecimalInput("Enter new transaction amount: ", true) },
            { "2", () => transaction.TransactionType = InputHandler.PromptEnum<TransactionType>("Enter new transaction type: ") },
            { "3", () => transaction.Description = InputHandler.PromptInput("Enter new transaction description: ", true) },
            { "4", () =>  transaction.Status = InputHandler.PromptEnum<Status>("Enter new transaction status: ") }
        };
        
        foreach (var option in fieldChoices.Keys) Console.WriteLine($"{option}. {GetFieldName(option)}");
        var choice = InputHandler.PromptInput("Enter your choice (1-10): ", true);
        
        if (!fieldChoices.TryGetValue(choice, out var action))
        {
            Console.WriteLine("Invalid Choice. No Changes Made.");
            return;
        }
        
        action();
    
        if (!ValidateTransaction(transaction)) return;

        try
        {
            context.Transactions.Update(transaction);
            context.SaveChanges();
            Console.WriteLine("Transaction updated successfully.");
            LoggerManager.LogSystemEvent("info", $"Transaction Updated. ID: {transaction.Id}, New Amount: {transaction.Amount}, New Status: {transaction.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating transaction: {ex.Message}");
            LoggerManager.LogSystemEvent("error", $"Failed to update transaction ID {transaction.Id}: {ex.Message}");
        }
    }

    public void DeleteTransaction()
    {
        if (!ValidateAdminAccess()) return;
        
        ReadAllTransactions();

        var transactionId = InputHandler.PromptIntInput("Enter the ID of the transaction to delete: ");
        var transaction = context.Transactions.FirstOrDefault(t => t.Id == transactionId);
        if (transaction == null)
        {
            Console.WriteLine("Transaction not found.");
            return;
        }

        try
        {
            context.Transactions.Remove(transaction);
            context.SaveChanges();
            Console.WriteLine("Transaction deleted successfully.");
            LoggerManager.LogSystemEvent("info", $"Transaction Deleted. ID: {transaction.Id}, Amount: {transaction.Amount}, Status: {transaction.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting transaction: {ex.Message}");
            LoggerManager.LogSystemEvent("error", $"Failed to delete transaction ID {transaction.Id}: {ex.Message}");
        }
    }

    
    private bool ValidateAdminAccess()
    {
        if (systemUser.Details!.IsAdmin) return true;
        Console.WriteLine("You cannot access transaction management section because you don't have admin rights.");
        return false;
    }

    private static bool ValidateTransaction(Transaction transaction)
    {
        var validationResult = new TransactionValidator().Validate(transaction);
        if (validationResult.IsValid) return true;
        
        foreach (var error in validationResult.Errors)
        {
            Console.WriteLine(error.ErrorMessage);
        }

        return false;
    }

    private static void PrintTransactions(List<Transaction> transactions, bool enablePrintingId = false)
    {
        foreach (var transaction in transactions)
        {
            if (enablePrintingId)
            {
                Console.Write($"ID: {transaction.Id}, ");
            }

            Console.Write($"Amount: {transaction.Amount} {transaction.Account!.Currency}, ");
            Console.Write($"Account Number: {transaction.Account.AccountNumber}, ");
            Console.Write($"Transaction Type: {transaction.TransactionType}, ");
            Console.Write($"Transaction Date: {transaction.TransactionDate}, ");
            Console.Write($"Status: {transaction.Status}, ");
            Console.Write($"Description: {transaction.Description}\n");
        }
    }

    private static string GetFieldName(string option)
    {
        return option switch
        {
            "1" => "Transaction Amount",
            "2" => "Transaction Type",
            "3" => "Transaction Description",
            "4" => "Transaction Status",
            _ => "Unknown Field"
        };
    }
}