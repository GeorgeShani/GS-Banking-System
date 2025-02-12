using Banking_System.Enums;

namespace Banking_System.Models;

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public Status Status { get; set; }
    
    public int AccountId { get; set; }
    public Account? Account { get; set; }
}