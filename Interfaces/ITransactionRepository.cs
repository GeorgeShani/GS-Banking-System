using Banking_System.Models;

namespace Banking_System.Interfaces;

public interface ITransactionRepository
{
    void CreateTransaction();
    void ReadTransactions();
    void ReadAllTransactions();
    void UpdateTransaction();
    void DeleteTransaction();
}