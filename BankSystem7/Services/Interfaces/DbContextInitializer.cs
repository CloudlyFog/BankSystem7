using BankSystem7.Services.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Interfaces;

public abstract class DbContextInitializer
{
    protected static void InitializeDbContexts(Dictionary<DbContext, ModelConfiguration> contexts)
    {
        foreach (var context in contexts)
            context.Key.GetType()?.GetConstructor(new [] { context.Value.GetType() })?.Invoke(new object[] { context.Value });
    }
}