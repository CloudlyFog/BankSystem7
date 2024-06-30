using BankSystem7.BankAggregate.BankAccountAggregate;
using BankSystem7.BankAggregate.CardAggregate;
using BankSystem7.BankAggregate.CreditAggregate;
using BankSystem7.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Configuration;

public class ModelConfiguration
{
    public virtual void Invoke(ModelBuilder modelBuilder, List<ModelConfiguration>? modelConfigurations = null)
    {
        ConfigureRelationships(modelBuilder);
        ConfigureDecimalColumnTypes(modelBuilder);

        modelBuilder.Entity<User>().Ignore(user => user.Exception);
        modelBuilder.Entity<User>().HasIndex(x => x.Id);

        InitializeModelConfigurations(modelBuilder, modelConfigurations);
    }

    private void InitializeModelConfigurations(ModelBuilder modelBuilder, List<ModelConfiguration>? modelConfigurations)
    {
        if (modelConfigurations is null || modelConfigurations.Count == 0)
            return;
        foreach (var modelConfiguration in modelConfigurations)
        {
            var method = modelConfiguration.GetType()?.GetMethod(nameof(Invoke));
            modelConfiguration.GetType()?.GetMethod(nameof(Invoke))?.Invoke(modelConfiguration, [modelBuilder, new List<ModelConfiguration>()]);
        }
    }

    private static void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        ConfigureCardRelationships(modelBuilder);
        ConfigureCreditRelationships(modelBuilder);
        ConfigureBankAccountRelationships(modelBuilder);
    }

    private static void ConfigureDecimalColumnTypes(ModelBuilder modelBuilder)
    {
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }
    }

    private static void ConfigureCreditRelationships(ModelBuilder modelBuilder)
    {
        // bank
        modelBuilder.Entity<Credit>()
            .HasOne(credit => credit.Bank)
            .WithMany(bank => bank.Credits)
            .HasForeignKey(credit => credit.BankId);

        // user
        modelBuilder.Entity<Credit>()
            .HasOne(credit => credit.User)
            .WithOne(user => user.Credit)
            .HasForeignKey<Credit>(credit => credit.UserId);
    }

    private static void ConfigureCardRelationships(ModelBuilder modelBuilder)
    {
        // user
        modelBuilder.Entity<Card>()
            .HasOne(card => card.User)
            .WithOne(user => user.Card)
            .HasForeignKey<Card>(card => card.UserId);
    }

    private static void ConfigureBankAccountRelationships(ModelBuilder modelBuilder)
    {
        // card
        modelBuilder.Entity<BankAccount>()
            .HasOne(account => account.Card)
            .WithOne(card => card.BankAccount)
            .HasForeignKey<Card>(card => card.BankAccountID)
            .OnDelete(DeleteBehavior.Cascade);

        // bank
        modelBuilder.Entity<BankAccount>()
            .HasOne(account => account.Bank)
            .WithMany(bank => bank.BankAccounts)
            .HasForeignKey(account => account.BankId);
    }
}