namespace Banking_System.UserInterface.Menu;

public static class ClientMenu
{
    public static void ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine(new string('=', 29));
        Console.WriteLine("          Main Menu          ");
        Console.WriteLine(new string('=', 29));
        Console.WriteLine("\n1) Manage Accounts");
        Console.WriteLine("2) Update Your Profile");
        Console.WriteLine("3) Financial Operations");
        Console.WriteLine("4) View Transaction History");
        Console.WriteLine("5) Exit the System");
        Console.Write("\nChoose an option (1-5): ");
    }
    
    public static void AccountManagementMenu()
    {
        Console.Clear();
        Console.WriteLine(new string('=', 43));
        Console.WriteLine("          Account Management Menu          ");
        Console.WriteLine(new string('=', 43));
        Console.WriteLine("\n1) Open a New Account");
        Console.WriteLine("2) Browse Your Accounts");
        Console.WriteLine("3) Modify Account Details");
        Console.WriteLine("4) Toggle Account Status");
        Console.WriteLine("5) Return to Main Menu");
        Console.Write("\nEnter your choice (1-5): ");
    }

    public static void FinancialOperationsMenu()
    {
        Console.Clear();
        Console.WriteLine(new string('=', 45));
        Console.WriteLine("          Financial Operations Menu          ");
        Console.WriteLine(new string('=', 45));
        Console.WriteLine("\n1) Deposit Funds");
        Console.WriteLine("2) Withdraw Funds");
        Console.WriteLine("3) Transfer Money");
        Console.WriteLine("4) Calculate Interest");
        Console.WriteLine("5) Currency Converter");
        Console.WriteLine("6) Return to Main Menu");
        Console.Write("\nChoose your action (1-6): ");
    }
}