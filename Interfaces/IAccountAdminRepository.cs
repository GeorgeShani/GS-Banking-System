namespace Banking_System.Interfaces;

public interface IAccountAdminRepository
{
    void AdminCreateAccount();
    void AdminReadAccounts();
    void AdminUpdateAccount();
    void DeleteAccount();
}