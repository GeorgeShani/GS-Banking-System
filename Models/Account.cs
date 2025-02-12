using Banking_System.Enums;

namespace Banking_System.Models;

public class Account
{
    public int Id { get; set; }
    public string? AccountNumber { get; set; }
    public decimal Balance { get; set; }
    public string? Currency { get; set; }
    public AccountType AccountType { get; set; }
    public bool IsActive { get; set; }
    public DateTime OpenDate { get; set; }
    
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public List<Transaction>? Transactions { get; set; }
}