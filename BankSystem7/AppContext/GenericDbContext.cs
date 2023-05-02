using BankSystem7.Models;
using BankSystem7.Services;
using BankSystem7.Services.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.AppContext;

public class GenericDbContext<TUser, TCard, TBankAccount, TBank, TCredit> : DbContext
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    public GenericDbContext()
    {
        DatabaseHandle();
    }

    public GenericDbContext(string connection)
    {
        ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>.SetConnection(connection);
        DatabaseHandle();
    }

    public GenericDbContext(ModelConfiguration modelConfiguration)
    {
        BankServicesOptions<User, Card, BankAccount, Bank, Credit>.ModelConfiguration = modelConfiguration ?? new ModelConfiguration();
        DatabaseHandle();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder
            .UseSqlServer(ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        BankServicesOptions<User, Card, BankAccount, Bank, Credit>.ModelConfiguration.Invoke(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// handle creating and deleting database
    /// </summary>
    private void DatabaseHandle()
    {
        if (!BankServicesOptions<User, Card, BankAccount, Bank, Credit>.InitializeAccess)
            return;
        if (BankServicesOptions<User, Card, BankAccount, Bank, Credit>.Ensured)
            return;
        if (BankServicesOptions<User, Card, BankAccount, Bank, Credit>.EnsureDeleted)
            Database.EnsureDeleted();
        if (BankServicesOptions<User, Card, BankAccount, Bank, Credit>.EnsureCreated)
            Database.EnsureCreated();
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Ensured = true;
        BankServicesOptions<User, Card, BankAccount, Bank, Credit>.InitializeAccess = false;
    }
}