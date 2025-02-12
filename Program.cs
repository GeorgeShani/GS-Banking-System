using Banking_System.Data;
using Banking_System.Core.File;
using Banking_System.Analytics;
using Banking_System.Repositories;
using Banking_System.Security.Authentication;
using Banking_System.UserInterface.Menu;

var dataContext = new DataContext();
var systemUser = AuthManager.HandleUserAuth();

var clientRepository = new ClientRepository(systemUser, dataContext);
var accountRepository = new AccountRepository(systemUser, dataContext);
var transactionRepository = new TransactionRepository(systemUser, dataContext);
var financialOperationsRepository = new FinancialOperationsRepository(systemUser, dataContext);

while (true)
{
    if (systemUser.Details!.IsAdmin)
    {
        AdminMenu.ShowMainMenu();
        switch (Console.ReadLine())
        {
            case "1":
                clientRepository.HandleAdminClientManagement();
                break;
            case "2":
                accountRepository.HandleAccountAdminManagement();
                break;
            case "3":
                transactionRepository.HandleTransactionManagement();
                break;
            case "4":
                FileManager.HandleFileManagement();
                break;
            case "5":
                AnalyticsManager.HandleAnalyticsManagement();
                break;
            case "6":
                Console.WriteLine("Exiting GS Bank. Goodbye...");
                return;
            default:
                Console.WriteLine("Invalid Choice. Please try again.");
                break;
        }
        
        MainMenuManager.KeyPressToContinue();
    }
    else
    {
        ClientMenu.ShowMainMenu();
        switch (Console.ReadLine())
        {
            case "1":
                accountRepository.HandleAccountManagement();
                break;
            case "2":
                clientRepository.UpdateProfile();
                break;
            case "3":
                financialOperationsRepository.HandleFinancialOperations();
                break;
            case "4":
                transactionRepository.ReadTransactions();
                break;
            case "5":
                Console.WriteLine("Exiting GS Bank. Goodbye...");
                return;
            default:
                Console.WriteLine("Invalid Choice. Please try again.");
                break;
        }
        
        MainMenuManager.KeyPressToContinue();
    }
}