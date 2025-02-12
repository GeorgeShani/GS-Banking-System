using Banking_System.Data;
using Banking_System.Enums;
using Banking_System.Interfaces;
using Banking_System.Models;
using Banking_System.Core.Input;
using Banking_System.Core.Logging;
using Banking_System.Communication.Email;
using Banking_System.Communication.Exchange;
using Banking_System.Security.Validation;
using Banking_System.UserInterface.Menu;
using Microsoft.EntityFrameworkCore;

namespace Banking_System.Repositories;

public class FinancialOperationsRepository(Client systemUser, DataContext context) : IFinancialOperationsRepository
{
    public void HandleFinancialOperations()
    {
        var exit = false;
        while (!exit)
        {
            ClientMenu.FinancialOperationsMenu();
            switch (Console.ReadLine())
            {
                case "1":
                    Deposit();
                    break;
                case "2":
                    Withdraw();
                    break;
                case "3":
                    Transfer();
                    break;
                case "4":
                    CalculateInterest();
                    break;
                case "5":
                    CurrencyConverter();
                    break;
                case "6":
                    exit = true;
                    break;
            }
            
            if (!exit) MainMenuManager.KeyPressToContinue();
        }
    }
    
    public void Deposit()
    {
        DisplayAccounts(systemUser);
        var chosenAccount = GetAccountChoice(systemUser, "deposit into");
        var amount = InputHandler.PromptDecimalInput("Enter the amount to deposit: ", true);
        var transaction = CreateTransaction(amount, TransactionType.Deposit,
            $"Deposited {amount} {chosenAccount!.Currency} into Account {chosenAccount.AccountNumber}", 
            Status.Pending, chosenAccount);

        if (!ValidateTransaction(transaction)) return;

        try
        {
            chosenAccount.Balance += amount;
            transaction.Status = Status.Completed;
            context.Accounts.Update(chosenAccount);
            context.Transactions.Add(transaction);
            context.SaveChanges();
            Console.WriteLine($"Successfully deposited {amount} {chosenAccount.Currency} into Account {chosenAccount.AccountNumber}. New Balance: {chosenAccount.Balance} {chosenAccount.Currency}.");
            LoggerManager.LogSystemEvent("info", $"Amount {amount} {chosenAccount.Currency} deposited successfully into Account {chosenAccount.AccountNumber}");
            LoggerManager.LogAccountStatement(systemUser, chosenAccount);
            EmailSender.SendEmail(systemUser.Details!.Email!, "Transaction Alert", EmailTemplates.TransactionAlertEmail(systemUser, chosenAccount, transaction));
        }
        catch (Exception ex)
        {
            chosenAccount.Balance -= amount;
            transaction.Status = Status.Failed;
            context.Accounts.Update(chosenAccount);
            context.Transactions.Add(transaction);
            context.SaveChanges();
            Console.WriteLine($"Error while deposit into the account: {ex}");
            LoggerManager.LogSystemEvent("error", $"Error depositing {amount} {chosenAccount.Currency} into Account {chosenAccount.AccountNumber}: {ex.Message}");
            LoggerManager.LogAccountStatement(systemUser, chosenAccount);
        }
    }

    public void Withdraw()
    {
        DisplayAccounts(systemUser);

        var chosenAccount = GetAccountChoice(systemUser, "withdraw from");
        var amount = InputHandler.PromptDecimalInput("Enter the amount to withdraw: ", true);
        if (!ValidateFunds(chosenAccount!, amount)) return;
        
        var transaction = CreateTransaction(amount, TransactionType.Withdrawal,
            $"Withdrew {amount} {chosenAccount!.Currency} from Account {chosenAccount.AccountNumber}", 
            Status.Pending, chosenAccount);
        
        if (!ValidateTransaction(transaction)) return;
        
        try
        {
            chosenAccount.Balance -= amount;
            transaction.Status = Status.Completed;
            context.Accounts.Update(chosenAccount);
            context.Transactions.Add(transaction);
            context.SaveChanges();
            Console.WriteLine($"Successfully withdrew {amount} {chosenAccount.Currency} from Account {chosenAccount.AccountNumber}. New Balance: {chosenAccount.Balance} {chosenAccount.Currency}.");
            LoggerManager.LogSystemEvent("info", $"Amount {amount} {chosenAccount.Currency} withdrawn successfully from Account {chosenAccount.AccountNumber}");
            LoggerManager.LogAccountStatement(systemUser, chosenAccount);
            EmailSender.SendEmail(systemUser.Details!.Email!, "Transaction Alert", EmailTemplates.TransactionAlertEmail(systemUser, chosenAccount, transaction));
        }
        catch (Exception ex)
        {
            chosenAccount.Balance += amount;
            transaction.Status = Status.Failed;
            context.Accounts.Update(chosenAccount);
            context.Transactions.Add(transaction);
            context.SaveChanges();
            Console.WriteLine($"Error while withdrawing from the account: {ex}");
            LoggerManager.LogSystemEvent("error", $"Error withdrawing {amount} {chosenAccount.Currency} from Account {chosenAccount.AccountNumber}: {ex.Message}");
            LoggerManager.LogAccountStatement(systemUser, chosenAccount);
        }
    }
    
    public void Transfer()
    {
        DisplayAccounts(systemUser);

        var clientSourceAccount = GetAccountChoice(systemUser, "transfer from");
        if (clientSourceAccount == null) return;
        
        var destinationAccount = GetReceiverAccount(PromptTransferMethod());
        if (destinationAccount == null) { Console.WriteLine("Receiver Account Not Found"); return; }
        
        // Validate if source and destination are the same account
        if (clientSourceAccount.AccountNumber == destinationAccount.AccountNumber) { Console.WriteLine("Cannot transfer to the same account."); return; }

        var transferAmount = InputHandler.PromptDecimalInput("Enter the amount to transfer: ", true);
        if (!ValidateFunds(clientSourceAccount, transferAmount)) return;

        var convertedTransferAmount = ConvertAmount(clientSourceAccount, destinationAccount, transferAmount);
        
        // Get fresh references to both accounts to avoid tracking conflicts
        var sourceAccount = GetFullAccountInfoById(clientSourceAccount.Id);
        var recipientAccount = GetFullAccountInfoById(destinationAccount.Id);
        if (sourceAccount == null || recipientAccount == null) { Console.WriteLine("Error: Could not retrieve account information."); return; }

        var outgoingTransaction = CreateTransaction(transferAmount, TransactionType.Transfer, $"Sent {transferAmount} {sourceAccount.Currency} to Account {recipientAccount.AccountNumber}", Status.Pending, sourceAccount);
        var incomingTransaction = CreateTransaction(convertedTransferAmount, TransactionType.Transfer, $"Received {convertedTransferAmount} {recipientAccount.Currency} from Account {sourceAccount.AccountNumber}", Status.Pending, recipientAccount);

        if (!ValidateTransaction(outgoingTransaction) || !ValidateTransaction(incomingTransaction)) return;

        try
        {
            sourceAccount.Balance -= transferAmount;
            recipientAccount.Balance += convertedTransferAmount;
            outgoingTransaction.Status = Status.Completed;
            incomingTransaction.Status = Status.Completed;
            context.Transactions.AddRange(new List<Transaction> { outgoingTransaction, incomingTransaction });
            context.SaveChanges();
            LoggerManager.LogSystemEvent("info", $"{transferAmount} {sourceAccount.Currency} successfully transferred from Account {sourceAccount.AccountNumber} to Account {recipientAccount.AccountNumber}");
            LoggerManager.LogAccountStatement(systemUser, sourceAccount);
            LoggerManager.LogAccountStatement(recipientAccount.Client!, recipientAccount);
            EmailSender.SendEmail(systemUser.Details!.Email!, "Transaction Alert", EmailTemplates.TransactionAlertEmail(systemUser, sourceAccount, outgoingTransaction));
            EmailSender.SendEmail(recipientAccount.Client!.Details!.Email!, "Transaction Alert", EmailTemplates.TransactionAlertEmail(recipientAccount.Client!, recipientAccount, incomingTransaction));
        }
        catch (Exception ex)
        {
            sourceAccount.Balance += transferAmount;
            recipientAccount.Balance -= convertedTransferAmount;
            outgoingTransaction.Status = Status.Failed;
            incomingTransaction.Status = Status.Failed;
            context.Transactions.AddRange(new List<Transaction> { outgoingTransaction, incomingTransaction });
            context.SaveChanges();
            Console.WriteLine($"Error transferring money: {ex}");
            LoggerManager.LogSystemEvent("error", $"Error transferring money from Account {sourceAccount.AccountNumber} to Account {recipientAccount.AccountNumber}: {ex.Message}");
        }
    }

    public void CalculateInterest()
    {
        DisplayAccounts(systemUser);   
        var chosenAccount = GetAccountChoice(systemUser, "get interest from");
        if (chosenAccount!.AccountType != AccountType.Savings)
        {
            Console.WriteLine("Interest is only calculated for Savings accounts.");
            return;
        }
        
        const decimal interestRate = 0.05m; // 5% annual interest rate
        var dailyInterest = chosenAccount.Balance * (interestRate / 365);

        Console.WriteLine($"Daily interest for account {chosenAccount.AccountNumber}: {dailyInterest:F2} {chosenAccount.Currency}");
        Console.WriteLine($"Monthly interest: {dailyInterest * 30:F2} {chosenAccount.Currency}");
        Console.WriteLine($"Yearly interest: {dailyInterest * 365:F2} {chosenAccount.Currency}");
    }

    public void CurrencyConverter()
    {
        var exchangeRates = StaticExchangeRateManager.GetExchangeRates();
        
        Console.WriteLine("Printing Exchange Rates");
        foreach (var exchangeRate in exchangeRates.Where(rate => rate.Key != "GEL"))
            Console.WriteLine($"{exchangeRate.Key}: {exchangeRate.Value} GEL");
        
        var sourceCurrency = InputHandler.PromptInput("Enter source currency (GEL/USD/EUR/GBP): ").ToUpper();
        var targetCurrency = InputHandler.PromptInput("Enter target currency (GEL/USD/EUR/GBP): ").ToUpper();
        var amount = InputHandler.PromptDecimalInput("Enter amount to convert to currency: ", true);
        
        if (!exchangeRates.ContainsKey(sourceCurrency) || !exchangeRates.ContainsKey(targetCurrency))
        {
            Console.WriteLine("Invalid currency.");
            return;
        }
        
        var sourceRate = exchangeRates[sourceCurrency];
        var targetRate = exchangeRates[targetCurrency];
        var convertedAmount = Math.Round(amount * (sourceRate / targetRate), 2);

        Console.WriteLine($"{amount} {sourceCurrency} = {convertedAmount:F2} {targetCurrency}");
    }
    
    
    private static void DisplayAccounts(Client client)
    {
        if (client.Accounts == null || client.Accounts.Count == 0)
        {
            Console.WriteLine("Accounts not found");
            return;
        }
        
        Console.WriteLine("Displaying accounts' info...");
        client.Accounts!
            .Where(a => a.IsActive)
            .Select((account, index) => $"{index + 1}. Account Number: {account.AccountNumber}; Balance: {account.Balance} {account.Currency}; Type: {account.AccountType};")
            .ToList()
            .ForEach(Console.WriteLine);
    }
    
    private static Account? GetAccountChoice(Client client, string action)
    {
        var accountIndex = InputHandler.PromptIntInput($"Enter the correspondent number for the account you want to {action}: ") - 1;
        if (IsValidAccountChoice(accountIndex, client.Accounts)) return client.Accounts![accountIndex];
        Console.WriteLine("Invalid account choice. Please select a valid account.");
        return null;
    }
    
    private static bool IsValidAccountChoice(int choice, List<Account>? accounts)
    {
        return accounts != null && choice >= 0 && choice < accounts.Count;
    }
    
    private Account? GetReceiverAccount(int wayToTransfer)
    {
        switch (wayToTransfer)
        {
            case 1:
                var personalNumber = InputHandler.PromptInput("Enter the Personal Number: ");
                return context.Clients
                    .Include(c => c.Accounts)!
                    .ThenInclude(a => a.Transactions)
                    .FirstOrDefault(c => c.PersonalNumber == personalNumber)?
                    .Accounts?.FirstOrDefault(a => a is { AccountType: AccountType.Current, IsActive: true });
            case 2:
                var accountNumber = InputHandler.PromptInput("Enter the Account Number: ");
                return context.Accounts
                    .Include(a => a.Client)
                    .Include(a => a.Transactions)
                    .FirstOrDefault(a => a.AccountNumber == accountNumber && a.IsActive);
            case 3:
                var phoneNumber = InputHandler.PromptInput("Enter the Phone Number: ");
                return context.Clients
                    .Include(c => c.Accounts)!
                    .ThenInclude(a => a.Transactions)
                    .FirstOrDefault(c => c.Details!.PhoneNumber == phoneNumber)?
                    .Accounts?.FirstOrDefault(a => a is { AccountType: AccountType.Current, IsActive: true });
            default:
                Console.WriteLine("Invalid transfer method selected.");
                return null;
        }
    }

    private Account? GetFullAccountInfoById(int accountId)
    {
        return context.Accounts
            .Include(a => a.Transactions)
            .Include(a => a.Client).ThenInclude(c => c!.Details)
            .FirstOrDefault(a => a.Id == accountId);
    }
    
    private static int PromptTransferMethod()
    {
        Console.WriteLine("How do you want to transfer?");
        Console.WriteLine("1. By Personal Number");
        Console.WriteLine("2. By Account Number");
        Console.WriteLine("3. By Phone Number");
        return InputHandler.PromptIntInput("Enter the way you want to transfer: ");
    }
    
    private static bool ValidateFunds(Account account, decimal amount)
    {
        if (account.Balance >= amount) return true;
        Console.WriteLine("Insufficient funds.");
        return false;
    }
    
    private static decimal ConvertAmount(Account senderAccount, Account receiverAccount, decimal amount)
    {
        return senderAccount.Currency != receiverAccount.Currency 
            ? ConvertAccountsCurrencies(senderAccount, receiverAccount, amount)
            : amount;
    }
    
    private static decimal ConvertAccountsCurrencies(Account senderAccount, Account receiverAccount, decimal amount)
    {
        var exchangeRates = StaticExchangeRateManager.GetExchangeRates();

        var senderRate = exchangeRates[senderAccount.Currency!];
        var receiverRate = exchangeRates[receiverAccount.Currency!];

        // Convert the amount from sender's currency to receiver's currency
        return Math.Round(amount * (senderRate / receiverRate), 2);
    }

    private static Transaction CreateTransaction(decimal amount, TransactionType transactionType, string description, Status status, Account account)
    {
        return new Transaction
        {
            Amount = amount,
            TransactionType = transactionType,
            TransactionDate = DateTime.Now,
            Description = description,
            Status = status,
            AccountId = account.Id,
            Account = account
        };
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
}