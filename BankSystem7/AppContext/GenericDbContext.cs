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
        if (ServicesSettings.EnsureDeleted)
            Database.EnsureDeleted();
        if (ServicesSettings.EnsureCreated)
            Database.EnsureCreated();
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Ensured = true;
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.InitializeAccess = false;
    }
    
    /// <summary>
    /// Method updates states of entity. You should use this method with method <see cref="AvoidChanges"/> for the best state tracking of entities
    /// </summary>
    /// <param name="item">entity whose state will be changed</param>
    /// <param name="state">future state of <see cref="item"/></param>
    /// <param name="action">action that will invoked after state of entity will be changed. Usually as action you should use method <see cref="AvoidChanges"/></param>
    /// <param name="context">context that will handle state changing</param>
    /// <typeparam name="T">type of <see cref="item"/></typeparam>
    public void UpdateTracker<T>(T item, EntityState state, Action? action, DbContext context)
    {
        context.ChangeTracker.Clear();
        context.Entry(item).State = state;
        action?.Invoke();
    }

    /// <summary>
    /// Method ensures that passed entities won't be changed during call method SaveChanges() 
    /// </summary>
    /// <param name="entities">entities that shouldn't be changed</param>
    public void AvoidChanges(object[]? entities, DbContext context)
    {
        if (entities is null || entities.Length == 0)
            return;
        
        foreach (var entity in entities)
        {
            if (entity is not null)
                context.Entry(entity).State = EntityState.Unchanged;
        }
    }
}