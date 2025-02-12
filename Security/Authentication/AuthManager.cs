using Banking_System.Data;
using Banking_System.Models;
using Banking_System.Core.Input;
using Banking_System.Core.Logging;
using Banking_System.Security.Validation;
using Banking_System.Communication.Email;
using Banking_System.UserInterface.Menu;
using Microsoft.EntityFrameworkCore;

namespace Banking_System.Security.Authentication;

public static class AuthManager
{
    private static readonly DataContext Context = new();

    public static Client HandleUserAuth()
    {
        MainMenuManager.DisplayIntroductoryMessage();
        var client = Console.ReadLine() switch
        {
            "1" => LogIn(),
            "2" => SignUp(),
            _ => null
        };

        if (client == null)
        {
            Console.WriteLine("Invalid Operation or Authentication Failed. Exiting the System.");
            Environment.Exit(0);
        }
    
        return client;
    }
    
    public static Client? SignUp()
    {
        var firstName = InputHandler.PromptInput("Enter Your First Name: ", true);
        var lastName = InputHandler.PromptInput("Enter Your Last Name: ", true);
        var username = InputHandler.PromptInput("Enter Your Username: ", true);
        var personalNumber = InputHandler.PromptInput("Enter Your Personal Number: ", true);
        var dateOfBirth = InputHandler.PromptDateTime("Enter Your Date of Birth (dd/MM/yyyy): ");
        var address = InputHandler.PromptInput("Enter Your Address: ", true);
        var phoneNumber = InputHandler.PromptInput("Enter Your Phone Number: ", true);
        var email = InputHandler.PromptInput("Enter Your Email: ", true);
        var password = InputHandler.PromptSecureInput("Enter Your Password: ");
        var isVipClient = InputHandler.PromptBooleanInput("Are you a VIP Client? (y/n): ");

        try
        {
            if (Context.Clients.Any(c => c.Details!.Email == email)) // Duplicate email check
            {
                Console.WriteLine("An account with this email already exists. Please use a different email.");
                return null;
            }
            
            if (Context.Clients.Any(c => c.Username == username)) // Duplicate username check
            {
                Console.WriteLine("An account with this username already exists. Please choose a different username.");
                return null;
            }
            
            var newClient = new Client // Create client object
            {
                FirstName = firstName,
                LastName = lastName,
                Username = username,
                PersonalNumber = personalNumber,
                RegistrationDate = DateTime.Today,
                Accounts = new List<Account>(),
                Details = new ClientDetails
                {
                    DateOfBirth = dateOfBirth,
                    Address = address,
                    PhoneNumber = phoneNumber,
                    Email = email,
                    Password = password,
                    IsVipClient = isVipClient,
                    IsAdmin = false
                }
            };
            
            if (!ValidateClient(newClient)) return null; // Validate the new client data
            newClient.Details.Password = BCrypt.Net.BCrypt.HashPassword(password); // Hash the password
            Context.Clients.Add(newClient);  // Save the new client via the data context
            Context.SaveChanges();
            Console.WriteLine("Sign up successful! Welcome aboard.");
            LoggerManager.LogSystemEvent("info", $"New client signed up with email: {email}");
            EmailSender.SendEmail(newClient.Details.Email, "Welcome to GS Bank", EmailTemplates.WelcomeEmail(newClient));
            return newClient;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during sign-up: {ex}");
            LoggerManager.LogSystemEvent("error", $"Sign-up failed for email: {email}. Error: {ex.Message}");
            return null;
        }
    }

    public static Client? LogIn()
    {
        const int maxAttempts = 3;
        while (true)
        {
            var email = InputHandler.PromptInput("Enter your email address: ");
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Email cannot be empty.");
                continue;
            }

            var client = Context.Clients
                .AsNoTracking()
                .Include(c => c.Details)
                .Include(c => c.Accounts)!
                .ThenInclude(a => a.Transactions)
                .FirstOrDefault(c => c.Details!.Email == email);

            if (client == null)
            {
                Console.WriteLine("Email not found. Exiting login process.");
                LoggerManager.LogSystemEvent("warning", $"Login attempt with invalid email: {email}");
                return null;
            }

            for (var attempts = 1; attempts <= maxAttempts; attempts++)
            {
                var password = InputHandler.PromptSecureInput("Enter your password: ");
                if (string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Password cannot be empty.");
                    continue;
                }

                if (BCrypt.Net.BCrypt.Verify(password, client.Details!.Password))
                {
                    Console.WriteLine($"Welcome back, {client.FirstName}!");
                    LoggerManager.LogSystemEvent("info", $"Client with email {email} logged in successfully.");
                    MainMenuManager.KeyPressToContinue();
                    return client;
                }

                Console.WriteLine("Invalid password.");
                LoggerManager.LogSystemEvent("warning", $"Failed login attempt for client with email: {email}");
                
                if (attempts == maxAttempts)
                {
                    if (InputHandler.PromptBooleanInput("You have entered the wrong password 3 times. Would you like to recover your password? (y/n): "))
                    {
                        RecoverPassword(email);
                        Console.WriteLine("Please try logging in again with your new password.");
                    }
                    else
                    {
                        Console.WriteLine("Exiting login process.");
                    }

                    return null;
                }
            }
        }
    }

    public static void RecoverPassword(string clientEmail = "")
    {
        var email = clientEmail == "" ? InputHandler.PromptInput("Enter your email: ") : clientEmail;
        var client = Context.Clients
            .Include(c => c.Details)
            .FirstOrDefault(c => c.Details!.Email == email);

        if (client == null) return;

        var oneTimePassword = GenerateOtpCode();
        EmailSender.SendEmail(
            client.Details!.Email!, 
            "OTP Verification", 
            EmailTemplates.OtpVerificationEmail(client, oneTimePassword)
        );
        
        Console.WriteLine("For verification, we have sent you an OTP on your email address");
        var confirmedOtp = InputHandler.PromptInput("Enter the OTP code: ");
        if (confirmedOtp != oneTimePassword) { Console.WriteLine("Verification failed. Please try again."); return; }

        var newPassword = InputHandler.PromptSecureInput("Enter your new password: ");
        var confirmedPassword = InputHandler.PromptSecureInput("Confirm your new password: ");
        if (newPassword != confirmedPassword) { Console.WriteLine("Password Do Not Match!"); return; }

        try
        {
            client.Details.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            Context.Clients.Update(client);
            Context.SaveChanges();
            Console.WriteLine("Password Changed Successfully.");
            LoggerManager.LogSystemEvent("info", $"Password Recovered Successfully. Client: {client.FirstName} {client.LastName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Recovering Password: {ex}");
            LoggerManager.LogSystemEvent("error", $"Error Recovering Password: {ex.Message}");
        }
    }
    
    private static bool ValidateClient(Client client)
    {
        var validationResult = new ClientValidator().Validate(client);
        if (validationResult.IsValid) return true;
        
        foreach (var error in validationResult.Errors)
        {
            Console.WriteLine(error.ErrorMessage);
        }

        return false;
    }

    private static string GenerateOtpCode()
    {
        var random = new Random();
        var code = "";

        for (var i = 0; i < 6; i++)
        {
            var digit = random.Next(0, 10);  // Generates a single digit between 0 and 9
            code += digit.ToString();          // Parses digit into a string and appends
        }
        
        return code;
    }
}