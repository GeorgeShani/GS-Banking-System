using System.Security;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Banking_System.Core.Input;

public static class InputHandler
{
    public static int PromptId(string type)
    {
        Console.Write($"Enter {type} ID: ");
        if (int.TryParse(Console.ReadLine(), out var id) && id > 0) return id;
        Console.WriteLine("Invalid ID entered. Please enter a positive integer.");
        return -1;
    }
    
    public static string PromptInput(string message, bool isRequired = false)
    {
        string input;
        do
        {
            Console.Write(message);
            input = Console.ReadLine() ?? string.Empty;

            if (isRequired && string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("This field is required.");
            }
        } while (isRequired && string.IsNullOrWhiteSpace(input));

        return input;
    }

    // Prompts the user for secure input and returns it as a regular string
    // In our case, we use it to secure prompt for password 
    public static string PromptSecureInput(string message)
    {
        // Display the provided message to prompt the user.
        Console.Write(message);

        // Initialize a SecureString to hold the input securely.
        var secureInput = new SecureString();
        ConsoleKeyInfo keyInfo;

        do
        {
            // Read a key press from the user without displaying it.
            keyInfo = Console.ReadKey(true);

            if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter)
            {
                // If it's not Backspace or Enter, add the character to the SecureString.
                secureInput.AppendChar(keyInfo.KeyChar);
            }
            else if (keyInfo.Key == ConsoleKey.Backspace && secureInput.Length > 0)
            {
                // If Backspace is pressed, remove the last character.
                secureInput.RemoveAt(secureInput.Length - 1);
            }
        } while (keyInfo.Key != ConsoleKey.Enter); // Continue until Enter is pressed.

        Console.WriteLine();

        // Convert the SecureString to a regular string and return it.
        return ConvertToString(secureInput);
    }

    public static int PromptIntInput(string message, bool allowNegative = false)
    {
        while (true)
        {
            Console.Write(message);
            var input = Console.ReadLine();
            if (int.TryParse(input, out var result))
            {
                if (allowNegative || result >= 0) return result;
                Console.WriteLine("Negative Numbers are not allowed. Please try again.");
                continue;

            }

            Console.WriteLine("Invalid input. Please enter a valid integer.");
        }
    }
    
    public static decimal PromptDecimalInput(string message, bool isRequired = false)
    {
        string? input;
        do
        {
            Console.Write(message);
            input = Console.ReadLine();

            if (isRequired && string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("This field is required.");
                continue;
            }

            if (decimal.TryParse(input, out var result))
            {
                if (result > 0) return result;
                Console.WriteLine("Negative numbers are not allowed. Please enter a valid positive decimal value.");
                continue;
            }

            Console.WriteLine("Invalid input. Please enter a valid decimal value.");
        } while (isRequired || !string.IsNullOrWhiteSpace(input));

        return 0;
    }
    
    public static DateTime PromptDateTime(string message, string format = "dd/MM/yyyy")
    {
        while (true)
        {
            Console.Write(message);
            var input = Console.ReadLine();
            
            if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            
            Console.WriteLine($"Invalid Date Format. Please enter the date in {format} format.");
        }
    }
    
    public static bool PromptBooleanInput(string message, string confirm = "y", string cancel = "n")
    {
        while (true)
        {
            Console.Write(message);
            var input = Console.ReadLine();

            if (string.Equals(input, confirm, StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(input, cancel, StringComparison.OrdinalIgnoreCase)) return false;

            Console.WriteLine($"Invalid input. Please enter '{confirm}' or '{cancel}'.");
        }
    }
    
    // The "where TEnum : struct, Enum" constraint ensures that:
    // 1. TEnum is a value type (struct), which enums are.
    // 2. TEnum is specifically an enum, preventing invalid types like int or string.
    // This guarantees compile-time safety and restricts the method to work only with enums.
    public static TEnum PromptEnum<TEnum>(string message) where TEnum : struct, Enum
    {
        while (true)
        {
            Console.WriteLine(message);
            Console.WriteLine("Available options:");

            // Display enumeration options in a friendly format (1-based index).
            var enumValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
            for (var i = 0; i < enumValues.Count; i++)
            {
                Console.WriteLine($"{i + 1}) {enumValues[i]}");
            }

            Console.Write("Enter your choice (e.g., 1, 2, etc.): ");
            var input = Console.ReadLine();

            // Try to parse the input as an index (1-based).
            if (int.TryParse(input, out var index) && index > 0 && index <= enumValues.Count)
            {
                return enumValues[index - 1];
            }

            Console.WriteLine("Invalid choice. Please enter a valid option.");
        }
    }

    // Converts a SecureString to a regular string.
    private static string ConvertToString(SecureString secureString)
    {
        // Ensure the secureString parameter is not null.
        ArgumentNullException.ThrowIfNull(secureString);

        // Initialize a pointer to hold the unmanaged memory reference.
        var unmanagedString = IntPtr.Zero;

        try
        {
            // Marshal the SecureString to an unmanaged Unicode string.
            unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);

            // Convert the unmanaged string to a managed string and return it.
            return Marshal.PtrToStringUni(unmanagedString)!;
        }
        finally
        {
            // Free the unmanaged memory, ensuring no sensitive data remains in memory.
            Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
        }
    }
}
