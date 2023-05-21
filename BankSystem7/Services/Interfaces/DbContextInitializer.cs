using BankSystem7.Services.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Interfaces;

public abstract class DbContextInitializer
{
    protected static void InitializeDbContexts(Dictionary<DbContext, ModelConfiguration> contexts)
    {
        foreach (var context in contexts)
        {
            if (Equals(context.Key, contexts.Keys.Last()))
                ModelCreatingOptions.LastModelConfiguration = true;
            context.Key.GetType()?.GetConstructor(new[] { context.Value.GetType() })?.Invoke(new object[] { context.Value });
        }
    }
}

public abstract class OptionsUpdater : DbContextInitializer
{
    protected virtual void UpdateOptions(ConfigurationOptions options)
    {
        UpdateEnsureOperations(options.ConnectionConfiguration.EnsureCreated, options.ConnectionConfiguration.EnsureDeleted);
        UpdateOnModelCreating(options.Contexts);
    }

    protected void UpdateConnection(string connection)
    {
        ServicesSettings.SetConnection(connection);
    }

    protected void UpdateDatabaseName(string databaseName)
    {
        ServicesSettings.SetConnection(databaseName: databaseName);
    }

    protected void UpdateEnsureOperations(bool ensureCreated, bool ensureDeleted)
    {
        ServicesSettings.EnsureCreated = ensureCreated;
        ServicesSettings.EnsureDeleted = ensureDeleted;
    }

    protected void UpdateOnModelCreating(Dictionary<DbContext, ModelConfiguration>? contexts)
    {
        if (contexts is null)
            throw new ArgumentNullException(nameof(contexts));

        InitializeDbContexts(contexts);
    }
}