namespace Banking_System.Models;

public class ClientDetails
{
    public int Id { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool IsVipClient { get; set; }
    public bool IsAdmin { get; set; }
    
    public int ClientId { get; set; }
    public Client? Client { get; set; }
}