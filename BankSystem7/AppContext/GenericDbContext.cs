using BankSystem7.Models;
using BankSystem7.Services;
using BankSystem7.Services.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.AppContext;

public class GenericDbContext : DbContext
{
    public GenericDbContext()
    {
        ModelCreatingOptions.ModelConfiguration = new ModelConfiguration();
        DatabaseHandle();
    }

    public GenericDbContext(string connection)
    {
        ServicesSettings.SetConnection(connection);
        ModelCreatingOptions.ModelConfiguration = new ModelConfiguration();
        DatabaseHandle();
    }

    public GenericDbContext(string connection, bool useOwnAccessConfiguration = false)
    {
        ServicesSettings.SetConnection(connection);
        ModelCreatingOptions.ModelConfiguration = new ModelConfiguration();
        ServicesSettings.InitializeAccess = useOwnAccessConfiguration;
        DatabaseHandle();
    }

    public GenericDbContext(ModelConfiguration? modelConfiguration)
    {
        ModelCreatingOptions.ModelConfigurations?.Add(modelConfiguration ?? new ModelConfiguration());
        ServicesSettings.InitializeAccess =
            modelConfiguration.InitializeAccess;
        if (ModelCreatingOptions.LastModelConfiguration)
            DatabaseHandle();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder
            .UseSqlServer(ServicesSettings.Connection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new ModelConfiguration().Invoke(modelBuilder, ModelCreatingOptions.ModelConfigurations);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// handle creating and deleting database
    /// </summary>
    private void DatabaseHandle()
    {
        if (!ServicesSettings.InitializeAccess)
            return;
        if (ServicesSettings.Ensured)
            return;
        if (ServicesSettings.EnsureDeleted)
            Database.EnsureDeleted();
        if (ServicesSettings.EnsureCreated)
            Database.EnsureCreated();
        ServicesSettings.Ensured = true;
        ServicesSettings.InitializeAccess = false;
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