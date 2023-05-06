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

    public GenericDbContext(string connection, bool useOwnAccessConfiguration = false)
    {
        ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>.SetConnection(connection);
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.InitializeAccess = useOwnAccessConfiguration;
        DatabaseHandle();
    }

    public GenericDbContext(ModelConfiguration? bankSystemModelConfiguration)
    {
        ModelCreatingOptions.ModelConfiguration = bankSystemModelConfiguration ?? new ModelConfiguration();
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.InitializeAccess =
            ModelCreatingOptions.ModelConfiguration.InitializeAccess;
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
        ModelCreatingOptions.ModelConfiguration.Invoke(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// handle creating and deleting database
    /// </summary>
    private void DatabaseHandle()
    {
        if (!BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.InitializeAccess)
            return;
        if (BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Ensured)
            return;
        if (BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.EnsureDeleted)
            Database.EnsureDeleted();
        if (BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.EnsureCreated)
            Database.EnsureCreated();
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Ensured = true;
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.InitializeAccess = false;
    }
    
    public void UpdateTracker<T>(T item, EntityState state, Action action, DbContext context)
    {
        context.ChangeTracker.Clear();
        context.Entry(item).State = state;
        action.Invoke();
    }
}