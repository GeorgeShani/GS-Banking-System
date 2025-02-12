using Banking_System.Data;
using Banking_System.Interfaces;
using Banking_System.Models;
using Banking_System.Core.Input;
using Banking_System.Core.Logging;
using Banking_System.Security.Validation;
using Banking_System.UserInterface.Menu;
using Microsoft.EntityFrameworkCore;

namespace Banking_System.Repositories;

public class ClientRepository(Client systemUser, DataContext context) : IClientRepository
{
    public void HandleAdminClientManagement()
    {
        var exit = false;
        while (!exit)
        {
            AdminMenu.ClientManagementMenu();
            switch (Console.ReadLine())
            {
                case "1":
                    CreateClient();
                    break;
                case "2":
                    ReadClients();
                    break;
                case "3":
                    ReadVipClients();
                    break;
                case "4":
                    UpdateClient();
                    break;
                case "5":
                    DeleteClient();
                    break;
                case "6":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Invalid choice, please try again.");
                    break;
            }
            
            if (!exit) MainMenuManager.KeyPressToContinue();
        }
    }

    // Profile Update can be accessed by a regular user (client)
    public void UpdateProfile()
    {
        Console.WriteLine("Displaying Profile Information...");
        Thread.Sleep(1200);
        
        Console.WriteLine($"Full Name: {systemUser.FirstName} {systemUser.LastName}");
        Console.WriteLine($"Username: {systemUser.Username}");
        Console.WriteLine($"Personal Number: {systemUser.PersonalNumber}");
        Console.WriteLine($"Date Of Birth: {systemUser.Details!.DateOfBirth}");
        Console.WriteLine($"Email: {systemUser.Details!.Email}");
        Console.WriteLine($"Phone Number: {systemUser.Details!.PhoneNumber}");
        Console.WriteLine($"Address: {systemUser.Details!.Address}");
        Console.WriteLine($"VIP Status: {(systemUser.Details.IsVipClient ? "Yes" : "No")}");
        MainMenuManager.KeyPressToContinue();
        
        UpdateClientFields(systemUser, false);
    }
    
    // Following CRUD Operations can be accessed only by an admin
    public void CreateClient()
    {
        if (!ValidateAdminAccess()) return;
        var firstName = InputHandler.PromptInput("Enter First Name: ", true);
        var lastName = InputHandler.PromptInput("Enter Last Name: ", true);
        var username = InputHandler.PromptInput("Enter Username: ", true);
        var personalNumber = InputHandler.PromptInput("Enter Personal Number: ", true);
        var dateOfBirth = InputHandler.PromptDateTime("Enter Date of Birth (dd/MM/yyyy): ");
        var address = InputHandler.PromptInput("Enter Address: ", true);
        var phoneNumber = InputHandler.PromptInput("Enter Phone Number: ", true);
        var email = InputHandler.PromptInput("Enter Email: ", true);
        var password = InputHandler.PromptInput("Enter Password: ", true);
        var isVipClient = InputHandler.PromptBooleanInput("Is Client a VIP Client? (y/n): ");
        var isAdmin = InputHandler.PromptBooleanInput("Is Client an Admin? (y/n): ");
        var registrationDate = DateTime.Today;

        if (context.Clients.Any(c => c.Details!.Email == email))
        {
            Console.WriteLine("An account with this email already exists. Please use a different email.");
            return;
        }

        if (context.Clients.Any(c => c.Username == username))
        {
            Console.WriteLine("An account with this username already exists. Please choose a different username.");
            return;
        }
        
        var client = new Client
        {
            FirstName = firstName,
            LastName = lastName,
            Username = username,
            PersonalNumber = personalNumber,
            RegistrationDate = registrationDate,
            Details = new ClientDetails
            {
                DateOfBirth = dateOfBirth,
                Address = address,
                PhoneNumber = phoneNumber,
                Email = email,
                Password = password,
                IsVipClient =  isVipClient,
                IsAdmin = isAdmin
            }
        };

        if (!ValidateClient(client)) return;

        try
        {
            client.Details.Password = BCrypt.Net.BCrypt.HashPassword(client.Details.Password);
            context.Clients.Add(client);
            context.SaveChanges();
            Console.WriteLine("Client Created Successfully");
            LoggerManager.LogSystemEvent("info", 
                $"New Client Created. First Name: {client.FirstName}, Last Name: {client.LastName}, " +
                $"Username: {client.Username}, Timestamp: {client.RegistrationDate}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating new client: " + ex);
            LoggerManager.LogSystemEvent("error", $"Failed to create a new client: {ex.Message}");
        }
    }

    public void ReadClients()
    {
        if (!ValidateAdminAccess()) return;
        if (!context.Clients.Any())
        {
            Console.WriteLine("No Clients In the Database.");
            return;
        }
        
        var clients = context.Clients.Include(c => c.Details).ToList();
        Console.WriteLine("Printing Clients...");
        PrintClients(clients);
    }

    public void ReadVipClients()
    {
        if (!ValidateAdminAccess()) return;
        if (!context.Clients.Any())
        {
            Console.WriteLine("No Clients In the Database.");
            return;
        }
        
        var vipClients = context.Clients
            .Include(c => c.Details)
            .Where(c => c.Details!.IsVipClient).ToList();
        
        Console.WriteLine("Printing VIP Clients...");
        PrintClients(vipClients);
    }

    public void UpdateClient()
    {
        if (!ValidateAdminAccess()) return;
        var id = InputHandler.PromptId("Client");
        if (id == -1) return;
    
        var client = context.Clients.Include(c => c.Details).FirstOrDefault(c => c.Id == id);
        if (client == null) { Console.WriteLine($"No Client Found with ID: {id}"); return; }

        UpdateClientFields(client, true);
    }

    public void DeleteClient()
    {
        if (!ValidateAdminAccess()) return;
        var id = InputHandler.PromptId("Client");
        if (id == -1) return;
        
        var client = context.Clients.Include(c => c.Details).FirstOrDefault(c => c.Id == id);
        if (client == null) { Console.WriteLine($"No Client Found with ID: {id}"); return; }

        try
        {
            Console.Write("Are you sure you want to delete this client? (y/n): ");
            var answer = Console.ReadLine();
            if (answer == "n") return;
                
            context.Clients.Remove(client);
            context.SaveChanges();
            LoggerManager.LogSystemEvent("info", 
                $"Client Deleted. First Name: {client.FirstName}, Last Name: {client.LastName}, " +
                $"Username: {client.Username}, Timestamp: {DateTime.Now}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error deleting client: " + ex.Message);
            LoggerManager.LogSystemEvent("error", $"Failed to delete client with ID {client.Id}: {ex.Message}");
        }
    }
    
    
    private void UpdateClientFields(Client client, bool isAdmin)
    {
        string? newPassword = null;
        
        // Define a dictionary where each key represents a field choice (1-10),
        // and the corresponding value is an Action that updates the chosen field.
        var fieldChoices = new Dictionary<string, Action>
        {
            { "1", () => client.FirstName = InputHandler.PromptInput("Enter new First Name: ", true) }, // Update First Name
            { "2", () => client.LastName = InputHandler.PromptInput("Enter new Last Name: ", true) },   // Update Last Name
            { "3", () => client.Username = InputHandler.PromptInput("Enter new Username: ", true) },   // Update Username
            { "4", () => client.PersonalNumber = InputHandler.PromptInput("Enter new Personal Number: ", true) }, // Update Personal Number
            { "5", () => client.Details!.DateOfBirth = InputHandler.PromptDateTime("Enter new Date of Birth (dd/MM/yyyy): ") }, // Update Date of Birth
            { "6", () => client.Details!.Address = InputHandler.PromptInput("Enter new Address: ", true) }, // Update Address
            { "7", () => client.Details!.PhoneNumber = InputHandler.PromptInput("Enter new Phone Number: ", true) }, // Update Phone Number
            { "8", () => client.Details!.IsVipClient = InputHandler.PromptBooleanInput("Enter new VIP Status (y/n): ") }, // Update VIP Status
            { "9", () => client.Details!.Email = InputHandler.PromptInput("Enter new Email: ", true) }, // Update Email
            { "10", () => newPassword = InputHandler.PromptInput("Enter new Password: ", true) }       // Update Password (stored in a temporary variable)
        };
        
        if (isAdmin)
        {
            fieldChoices.Add("11", () => client.Details!.IsAdmin = InputHandler.PromptBooleanInput("Enter new Admin Status (y/n): "));
        }

        Console.WriteLine("\nSelect the field you want to update:");
        // Loop through the dictionary keys and display each option to the user. Use GetFieldName to print a user-friendly field name.
        foreach (var option in fieldChoices.Keys) Console.WriteLine($"{option}. {GetFieldName(option)}");
        var choice = InputHandler.PromptInput("Enter your choice (1-11): ", true);

        // Check if the entered choice exists in the dictionary.
        if (!fieldChoices.TryGetValue(choice, out var action))
        {
            // If the choice is invalid, display an error message and exit the method.
            Console.WriteLine("Invalid Choice. No Changes Made.");
            return;
        }

        // Execute the action corresponding to the user's choice to update the field.
        action();
        
        if (!ValidateClient(client)) return;

        try
        {
            if (!string.IsNullOrEmpty(newPassword))
            {
                client.Details!.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }
            context.Clients.Update(client);
            context.SaveChanges();
            LoggerManager.LogSystemEvent("info", $"Client with ID {client.Id} was Successfully Updated.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error updating client: " + ex);
            LoggerManager.LogSystemEvent("error", $"Failed to update client with ID {client.Id}: {ex.Message}");
        }
    }
    
    private bool ValidateAdminAccess()
    {
        if (systemUser.Details!.IsAdmin) return true;
        Console.WriteLine("You cannot access transaction management section because you don't have admin rights.");
        return false;
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

    private static void PrintClients(List<Client> clients)
    {
        foreach (var client in clients)
        {
            Console.WriteLine($"ID: {client.Id}");
            Console.WriteLine($"Full Name: {client.FirstName} {client.LastName}");
            Console.WriteLine($"Username: {client.Username}");
            Console.WriteLine($"Personal Number: {client.PersonalNumber}");
            Console.WriteLine($"Phone Number: {client.Details!.PhoneNumber}");
            Console.WriteLine($"Email: {client.Details!.Email}\n");
        }
    } 
    
    private static string GetFieldName(string option)
    {
        return option switch
        {
            "1" => "First Name",
            "2" => "Last Name",
            "3" => "Username",
            "4" => "Personal Number",
            "5" => "Date of Birth",
            "6" => "Address",
            "7" => "Phone Number",
            "8" => "VIP Status",
            "9" => "Admin Status",
            "10" => "Email",
            "11" => "Password",
            _ => "Unknown Field"
        };
    }
}
