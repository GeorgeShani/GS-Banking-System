namespace Banking_System.UserInterface.Menu;

public static class MainMenuManager
{
    public static void DisplayIntroductoryMessage()
    {
        Console.Clear();
        Console.WriteLine(new string('=', 47));
        Console.WriteLine("              Welcome to GS Bank!              ");
        Console.WriteLine(new string('=', 47));
        Console.WriteLine("GS Bank - Your trusted partner in managing your finances.");
        Console.WriteLine("With our mini banking system, you can:");
        Console.WriteLine("-> Open and manage your personal or business accounts effortlessly.");
        Console.WriteLine("-> Conduct secure transactions anytime, anywhere.");
        Console.WriteLine("-> Access exclusive VIP services for premium clients.");
        Console.WriteLine();
        Console.WriteLine("Existing clients can log in to manage their accounts.");
        Console.WriteLine("New to GS Bank? Sign up today and join our growing family.");
        Console.WriteLine(new string('=', 47));
        Console.WriteLine("Please select an option:");
        Console.WriteLine("1) Log In");
        Console.WriteLine("2) Sign Up");
        Console.Write("Enter your choice (1 or 2): ");
    }
    
    public static void KeyPressToContinue()
    {
        Console.Write("Press any key to continue...  ");
        Console.ReadKey();
    }
}