namespace Banking_System.Interfaces;

public interface IFinancialOperationsRepository
{
    void Deposit();
    void Withdraw();
    void Transfer();
    void CalculateInterest();
    void CurrencyConverter();
}