using Banking_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Banking_System.Data;

public class DataContext : DbContext
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientDetails> ClientDetails { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\ProjectModels;Database=GSBank");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>()
            .HasOne(c => c.Details)
            .WithOne(cd => cd.Client)
            .HasForeignKey<ClientDetails>(cd => cd.ClientId);

        modelBuilder.Entity<Client>()
            .HasMany(c => c.Accounts)
            .WithOne(a => a.Client)
            .HasForeignKey(a => a.ClientId);

        modelBuilder.Entity<Account>()
            .HasMany(a => a.Transactions)
            .WithOne(t => t.Account)
            .HasForeignKey(t => t.AccountId);

        modelBuilder.Entity<Account>()
            .Property(a => a.Balance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);
    }
}