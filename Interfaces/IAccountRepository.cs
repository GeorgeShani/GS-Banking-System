namespace Banking_System.Interfaces;

public interface IAccountRepository
{
    void CreateAccount();
    void ReadAccounts();
    void UpdateAccount();
    void ToggleAccountStatus();
}