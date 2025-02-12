namespace Banking_System.UserInterface.Menu;

public static class AdminMenu
{
    public static void ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine(new string('=', 30));
        Console.WriteLine("          Admin Menu          ");
        Console.WriteLine(new string('=', 30));
        Console.WriteLine("\n1) Manage Clients");
        Console.WriteLine("2) Handle Accounts");
        Console.WriteLine("3) Oversee Transactions");
        Console.WriteLine("4) Organize System Files");
        Console.WriteLine("5) Analyze Data");
        Console.WriteLine("6) Exit the System");
        Console.Write("\nChoose an option (1-6): ");
    }

    public static void ClientManagementMenu()
    {
        Console.Clear();
        Console.WriteLine(new string('=', 42));
        Console.WriteLine("          Client Management Menu          ");
        Console.WriteLine(new string('=', 42));
        Console.WriteLine("\n1) Add New Client");
        Console.WriteLine("2) View All Clients");
        Console.WriteLine("3) Highlight VIP Clients");
        Console.WriteLine("4) Edit Client Information");
        Console.WriteLine("5) Remove a Client");
        Console.WriteLine("6) Return to Main Menu");
        Console.Write("\nSelect an option (1-6): ");
    }

    public static void AccountManagementMenu()
    {
        Console.Clear();
        Console.WriteLine(new string('=', 43));
        Console.WriteLine("          Account Management Menu          ");
        Console.WriteLine(new string('=', 43));
        Console.WriteLine("\n1) Open a New Account");
        Console.WriteLine("2) Browse Existing Accounts");
        Console.WriteLine("3) Modify Account Details");
        Console.WriteLine("4) Delete an Account");
        Console.WriteLine("5) Return to Main Menu");
        Console.Write("\nPick an option (1-5): ");
    }

    public static void TransactionManagementMenu()
    {
        Console.Clear();
        Console.WriteLine(new string('=', 47));
        Console.WriteLine("          Transaction Management Menu          ");
        Console.WriteLine(new string('=', 47));
        Console.WriteLine("\n1) Initiate a Transaction");
        Console.WriteLine("2) Review Past Transactions");
        Console.WriteLine("3) Update Transaction Records");
        Console.WriteLine("4) Cancel a Transaction");
        Console.WriteLine("5) Return to Main Menu");
        Console.Write("\nChoose your action (1-5): ");
    }

    public static void FileManagementMenu()
    {
        Console.Clear();
        Console.WriteLine(new string('=', 40));
        Console.WriteLine("          File Management Menu          ");
        Console.WriteLine(new string('=', 40));
        Console.WriteLine("\n1) View System Logs");
        Console.WriteLine("2) Get Account Statement");
        Console.WriteLine("3) Return to Main Menu");
        Console.Write("\nEnter your choice (1-3): ");
    }

    public static void AnalyticsManagementMenu()
    {
        Console.Clear();
        Console.WriteLine(new string('=', 45));
        Console.WriteLine("          Analytics Management Menu          ");
        Console.WriteLine(new string('=', 45));
        Console.WriteLine("\n1) Clients with Multiple Accounts");
        Console.WriteLine("2) Transactions by Date Range");
        Console.WriteLine("3) Most Active Client (by Transactions)");
        Console.WriteLine("4) Top 5 Largest Transactions");
        Console.WriteLine("5) Transactions by Status");
        Console.WriteLine("6) Accounts Opened Recently");
        Console.WriteLine("7) Most Active Accounts");
        Console.WriteLine("8) Top Clients by Balance");
        Console.WriteLine("9) Daily Transaction Summary");
        Console.WriteLine("10) Account Stats by Currency");
        Console.WriteLine("11) Return to Main Menu");
        Console.Write("\nEnter your choice (1-11): ");
    }
}