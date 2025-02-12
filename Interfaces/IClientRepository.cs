using Banking_System.Models;

namespace Banking_System.Interfaces;

public interface IClientRepository
{
    void CreateClient();
    void ReadClients();
    void ReadVipClients();
    void UpdateClient();
    void DeleteClient();
}