using Banking_System.Data;
using Banking_System.Enums;
using Banking_System.Interfaces;
using Banking_System.Models;
using Banking_System.Core.Input;
using Banking_System.Core.Logging;
using Banking_System.Core.AccountUtils;
using Banking_System.Security.Validation;
using Banking_System.UserInterface.Menu;
using Microsoft.EntityFrameworkCore;

namespace Banking_System.Repositories;

public class AccountRepository(Client systemUser, DataContext context) : IAccountRepository, IAccountAdminRepository
{
    public void HandleAccountAdminManagement()
    {
        if (!systemUser.Details!.IsAdmin) return;
        
        var exit = false;
        while (!exit)
        {
            AdminMenu.AccountManagementMenu();
            switch (Console.ReadLine())
            {
                case "1":
                    AdminCreateAccount();
                    break;
                case "2":
                    AdminReadAccounts();
                    break;
                case "3":
                    AdminUpdateAccount();
                    break;
                case "4":
                    DeleteAccount();
                    break;
                case "5":
                    exit = true;
                    break;
            }
            
            if (!exit) MainMenuManager.KeyPressToContinue();
        }
    }
    
    public void HandleAccountManagement()
    {
        var exit = false;
        while (!exit)
        {
            ClientMenu.AccountManagementMenu();
            switch (Console.ReadLine())
            {
                case "1":
                    CreateAccount();
                    break;
                case "2":
                    ReadAccounts();
                    break;
                case "3":
                    UpdateAccount();
                    break;
                case "4":
                    ToggleAccountStatus();
                    break;
                case "5":
                    exit = true;
                    break;
            }
            
            if (!exit) MainMenuManager.KeyPressToContinue();
        }
    }
    
    public void CreateAccount()
    {
        var initialBalance = InputHandler.PromptDecimalInput("Enter Initial Balance: ", true);
        var currency = InputHandler.PromptInput("Enter Currency: ", true);
        var accountType = InputHandler.PromptEnum<AccountType>("Enter Account Type: ");
        var accountNumber = AccountNumberGenerator.GenerateAccountNumber();
        var openDate = DateTime.Now;
        const bool isActive = true;

        var newAccount = new Account
        {
            AccountNumber = accountNumber,
            Balance = initialBalance,
            Currency = currency,
            AccountType = accountType,
            IsActive = isActive,
            OpenDate = openDate,
            ClientId = systemUser.Id,
            Client = systemUser,
            Transactions = new List<Transaction>()
        };

        if (!ValidateAccount(newAccount)) return;

        try
        {
            systemUser.Accounts!.Add(newAccount);
            context.Clients.Update(systemUser);
            context.SaveChanges();
            LoggerManager.LogAccountStatement(systemUser, newAccount);
            LoggerManager.LogSystemEvent("info", $"New Account {newAccount.AccountNumber} created");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating new account: " + ex);
            LoggerManager.LogSystemEvent("error", $"Failed to create a new account: {ex.Message}");
        }
    }

    public void AdminCreateAccount()
    {
        var clientId = InputHandler.PromptId("Client");
        if (clientId == -1) return;
        
        var client = context.Clients
            .Include(c => c.Details)
            .Include(c => c.Accounts)
            .FirstOrDefault(c => c.Id == clientId);

        if (client == null) return;
        
        var initialBalance = InputHandler.PromptDecimalInput("Enter Initial Balance: ", true);
        var currency = InputHandler.PromptInput("Enter Currency: ", true);
        var accountType = InputHandler.PromptEnum<AccountType>("Enter Account Type: ");
        var accountNumber = AccountNumberGenerator.GenerateAccountNumber();
        var openDate = DateTime.Now;
        const bool isActive = true;

        var newAccount = new Account
        {
            AccountNumber = accountNumber,
            Balance = initialBalance,
            Currency = currency,
            AccountType = accountType,
            IsActive = isActive,
            OpenDate = openDate
        };

        if (!ValidateAccount(newAccount)) return;

        try
        {
            client.Accounts!.Add(newAccount);
            context.Clients.Update(client);
            context.SaveChanges();
            LoggerManager.LogAccountStatement(systemUser, newAccount);
            LoggerManager.LogSystemEvent("info", $"New Account {newAccount.AccountNumber} created");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating new account: " + ex);
            LoggerManager.LogSystemEvent("error", $"Failed to create a new account: {ex.Message}");
        }
    }

    public void ReadAccounts()
    {
        if (systemUser.Accounts == null || systemUser.Accounts!.Count == 0)
        {
            Console.WriteLine("No accounts to display...");
            return;
        }
        
        var systemUserAccounts = systemUser.Accounts!.Where(account => account.IsActive).ToList();
        for (var i = 0; i < systemUserAccounts.Count; i++)
        {
            var account = systemUserAccounts[i];
            Console.Write($"{i + 1}. Account Number: {account.AccountNumber}, ");
            Console.Write($"Account Type: {account.AccountType}, ");
            Console.Write($"Balance: {account.Balance} {account.Currency}, ");
            Console.Write($"Open Date: {account.OpenDate:dd/MM/yyyy}\n");
        }
    }

    public void AdminReadAccounts()
    {
        var allAccounts = context.Accounts.Include(a => a.Client).ToList();
        for (var i = 0; i < allAccounts.Count; i++)
        {
            var account = allAccounts[i];
            Console.Write($"{i + 1}. Account Number: {account.AccountNumber}, ");
            Console.Write($"Account Type: {account.AccountType}, ");
            Console.Write($"Balance: {account.Balance} {account.Currency}, ");
            Console.Write($"Status: {(account.IsActive ? "Active" : "Inactive")}, ");
            Console.Write($"Open Date: {account.OpenDate:dd/MM/yyyy}, ");
            Console.Write($"Client: {account.Client!.FirstName} {account.Client!.LastName}\n");
        }
    }

    public void UpdateAccount()
    {
        Console.WriteLine("Select an account to update:");
        ReadAccounts();

        var selectedIndex = InputHandler.PromptIntInput("Enter the corresponding number of the account from the list above to update: ") - 1;
        if (selectedIndex < 0 || selectedIndex >= systemUser.Accounts!.Count)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        var accountToUpdate = systemUser.Accounts[selectedIndex];
        
        Console.WriteLine("1. Update Account Status");
        Console.WriteLine("2. Update Account Type");
        switch (InputHandler.PromptIntInput("Select an option: "))
        {
            case 1:
                accountToUpdate.IsActive = InputHandler.PromptBooleanInput("Is the Account Active? (y/n): ");
                break;
            case 2:
                accountToUpdate.AccountType = InputHandler.PromptEnum<AccountType>("Enter new Account Type: ");
                break;
            default:
                Console.WriteLine("Invalid option.");
                return;
        }

        try
        {
            context.Accounts.Update(accountToUpdate);
            context.SaveChanges();
            Console.WriteLine("Account Updated Successfully.");
            LoggerManager.LogAccountStatement(systemUser, accountToUpdate);
            LoggerManager.LogSystemEvent("info", $"Account {accountToUpdate.AccountNumber} Updated Successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating account: {ex.Message}");
            LoggerManager.LogSystemEvent("error", $"Failed to update account: {ex.Message}");
        }
    }
    
    public void AdminUpdateAccount()
    {
        Console.WriteLine("Available Accounts:");
        var accounts = context.Accounts.Include(a => a.Client).ToList();
        AdminReadAccounts();

        var selectedIndex = InputHandler.PromptIntInput("Enter the corresponding number of the account from the list above to update: ") - 1;
        if (selectedIndex < 0 || selectedIndex >= accounts.Count)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        var accountToUpdate = accounts[selectedIndex];

        Console.WriteLine("Selected Account Details:");
        Console.WriteLine($"1. Account Number: {accountToUpdate.AccountNumber}");
        Console.WriteLine($"2. Balance: {accountToUpdate.Balance}");
        Console.WriteLine($"3. Currency: {accountToUpdate.Currency}");
        Console.WriteLine($"4. Account Type: {accountToUpdate.AccountType}");
        Console.WriteLine($"5. Account Status: {(accountToUpdate.IsActive ? "Active" : "Inactive")}");

        switch (InputHandler.PromptIntInput("Select a field to update (1-5): "))
        {
            case 1:
                accountToUpdate.AccountNumber = AccountNumberGenerator.GenerateAccountNumber();
                break;
            case 2:
                accountToUpdate.Balance = InputHandler.PromptDecimalInput("Enter new Balance: ");
                break;
            case 3:
                accountToUpdate.Currency = InputHandler.PromptInput("Enter new Currency: ", true);
                break;
            case 4:
                accountToUpdate.AccountType = InputHandler.PromptEnum<AccountType>("Enter new Account Type: ");
                break;
            case 5:
                accountToUpdate.IsActive = InputHandler.PromptBooleanInput("Is the Account Active? (y/n): ");
                break;
            default:
                Console.WriteLine("Invalid option.");
                return;
        }

        try
        {
            context.Accounts.Update(accountToUpdate);
            context.SaveChanges();
            Console.WriteLine("Account Updated Successfully.");
            LoggerManager.LogAccountStatement(accountToUpdate.Client!, accountToUpdate);
            LoggerManager.LogSystemEvent("info", $"Admin updated account {accountToUpdate.AccountNumber}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating account: {ex.Message}");
            LoggerManager.LogSystemEvent("error", $"Admin failed to update account: {ex.Message}");
        }
    }
    
    public void ToggleAccountStatus()
    {
        if (systemUser.Accounts == null || systemUser.Accounts.Count == 0)
        {
            Console.WriteLine("No accounts found.");
            return;
        }

        var clientAccounts = systemUser.Accounts.ToList();
        Console.WriteLine("Select an account to toggle its status (activate/deactivate):");
        for (var i = 0; i < clientAccounts.Count; i++)
        {
            var account = clientAccounts[i];
            Console.WriteLine($"{i + 1}. Account Number: {account.AccountNumber}, Balance: {account.Balance}, Type: {account.AccountType}, Active: {account.IsActive}");
        }

        var selectedIndex = InputHandler.PromptIntInput("Enter the corresponding number of the account from the list above to toggle the status: ") - 1;
        if (selectedIndex < 0 || selectedIndex >= clientAccounts.Count)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        var selectedAccount = clientAccounts[selectedIndex];
        selectedAccount.IsActive = !selectedAccount.IsActive; // Toggle the status

        try
        {
            context.Accounts.Update(selectedAccount);
            context.SaveChanges();
            var status = selectedAccount.IsActive ? "activated" : "deactivated";
            Console.WriteLine($"Account {selectedAccount.AccountNumber} successfully {status}.");
            LoggerManager.LogSystemEvent("info", $"Account {selectedAccount.AccountNumber} {status}.");
            LoggerManager.LogAccountStatement(systemUser, selectedAccount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error toggling account status: {ex.Message}");
            LoggerManager.LogSystemEvent("error", $"Failed to toggle account status: {ex.Message}");
        }
    }
    
    public void DeleteAccount()
    {
        var accounts = systemUser.Details!.IsAdmin ? context.Accounts.ToList() : systemUser.Accounts!.ToList();
        Console.WriteLine("Select an account to delete:");
        if (systemUser.Details.IsAdmin) AdminReadAccounts(); else ReadAccounts();

        var selectedIndex = InputHandler.PromptIntInput("Enter the corresponding number of the account from the list above to delete: ") - 1;
        if (selectedIndex < 0 || selectedIndex >= accounts.Count)
        {
            Console.WriteLine("Invalid selection.");
            return;
        }

        var accountToDelete = accounts[selectedIndex];
    
        try
        {
            context.Accounts.Remove(accountToDelete);
            context.SaveChanges();
            Console.WriteLine($"Account {accountToDelete.AccountNumber} successfully deleted.");
            LoggerManager.LogSystemEvent("info", $"Account {accountToDelete.AccountNumber} deleted.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting account: {ex.Message}");
            LoggerManager.LogSystemEvent("error", $"Failed to delete account: {ex.Message}");
        }
    }
    
    private static bool ValidateAccount(Account account)
    {
        var validationResult = new AccountValidator().Validate(account);
        if (validationResult.IsValid) return true;
        
        foreach (var error in validationResult.Errors)
        {
            Console.WriteLine(error.ErrorMessage);
        }

        return false;
    }
}