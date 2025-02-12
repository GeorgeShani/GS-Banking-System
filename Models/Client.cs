namespace Banking_System.Models;

public class Client
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? PersonalNumber { get; set; }
    public DateTime RegistrationDate { get; set; }
    
    public ClientDetails? Details { get; set; }
    public List<Account>? Accounts { get; set; }
}