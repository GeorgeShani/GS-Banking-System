namespace Banking_System.Core.AccountUtils;

public static class AccountNumberGenerator
{
    private static readonly Random Random = new();

    // Generates a valid 16-digit account number.
    // Ensures the account number has a valid prefix and checksum digit.
    public static string GenerateAccountNumber()
    {
        string accountNumberBase;
        int checksumDigit;

        do
        {
            // Generate a valid prefix (3, 4, 5 or 6).
            var prefix = GenerateValidPrefix();
            
            // Generate the base of the account number (prefix + 14 random digits).
            accountNumberBase = prefix + GenerateRandomDigits(14);
            
            // Calculate the checksum digit for the account number base.
            checksumDigit = CalculateChecksum(accountNumberBase);
        } 
        // Verify that the full account number (base + checksum) passes checksum validation.    
        while (!IsChecksumValid(accountNumberBase + checksumDigit));

        // Return the complete account number with the checksum digit appended.
        return accountNumberBase + checksumDigit;
    }

    // Generates a valid prefix for the account number.
    // The prefix must be one of the digits 3, 4, 5 or 6
    private static string GenerateValidPrefix()
    {
        var validPrefixes = new[] { "3", "4", "5", "6" };
        return validPrefixes[Random.Next(0, validPrefixes.Length)];
    }

    // Generates a string of random numeric digits with the specified length.
    private static string GenerateRandomDigits(int length)
    {
        // Use LINQ's Enumerable.Range to create a sequence of integers from 0 to (length - 1).
        // For each item in the sequence, generate a random digit between 0 and 9, 
        // convert it to a string, and concatenate them into a single string.
        return string.Concat(Enumerable.Range(0, length).Select(_ => Random.Next(0, 10).ToString()));
    }

    // Calculates the checksum digit for a given account number using the Luhn algorithm.
    // This digit is the final digit of a valid account number that ensures the entire number 
    // passes the checksum validation. It helps in error detection, ensuring the integrity 
    // of the account number during data entry or transmission.
    private static int CalculateChecksum(string accountNumber)
    {
        // Perform the Luhn algorithm to get the sum of the digits according to its rules.
        var sum = PerformLuhnAlgorithm(accountNumber);

        // The checksum digit is calculated to make the sum a multiple of 10.
        // If the sum's remainder when divided by 10 is zero, the checksum digit is 0.
        // Otherwise, it is the difference between 10 and the remainder, ensuring the total 
        // sum (including the checksum) is divisible by 10.
        return (10 - sum % 10) % 10;
    }

    // Validates an account number using the Luhn algorithm to check its checksum validity.
    // This method verifies that the account number, including its checksum digit, 
    // is correct by ensuring that the sum of its digits (as modified by the Luhn algorithm)
    // is a multiple of 10. This is a common technique for validating numbers like credit cards.
    private static bool IsChecksumValid(string accountNumber)
    {
        // Perform the Luhn algorithm to get the sum of the digits as per the algorithm's rules.
        var sum = PerformLuhnAlgorithm(accountNumber);

        // A valid account number will have a sum that is a multiple of 10, which indicates that
        // the number passes the checksum validation.
        return sum % 10 == 0;
    }
    
    private static int PerformLuhnAlgorithm(string accountNumber)
    {
        var sum = 0;
        var doubleDigit = false; // Indicates whether the current digit should be doubled.
        
        // Iterate over the account number digits from right to left.
        for (var i = accountNumber.Length - 1; i >= 0; i--)
        {
            var digit = accountNumber[i] - '0'; // Convert the character digit to an integer.
            
            if (doubleDigit)
            {
                digit *= 2; // Double the digit.
                sum += digit / 10 + digit % 10; // Add the sum of the digits of the product.
            }
            else
            {
                sum += digit; // Add the digit directly if not doubled.
            }
            
            doubleDigit = !doubleDigit; // Toggle the doubleDigit flag for the next iteration.
        }

        return sum;
    }
}
