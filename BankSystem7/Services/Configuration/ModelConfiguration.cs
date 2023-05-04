using BankSystem7.Models;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Configuration;

public class ModelConfiguration
{
    public ModelConfiguration()
    {
    }
    public ModelConfiguration(bool initializeAccess)
    {
        InitializeAccess = initializeAccess;
    }
    
    
    public bool InitializeAccess { get; }
    
    public virtual void Invoke(ModelBuilder modelBuilder)
    {
        ConfigureRelationships(modelBuilder);
        ConfigureDecimalColumnTypes(modelBuilder);

        modelBuilder.Entity<User>().Ignore(user => user.Exception);
        modelBuilder.Entity<User>().HasIndex(x => x.ID);
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
            .HasForeignKey(credit => credit.BankID);

        // user
        modelBuilder.Entity<Credit>()
            .HasOne(credit => credit.User)
            .WithOne(user => user.Credit)
            .HasForeignKey<Credit>(credit => credit.UserID);
    }

    private static void ConfigureCardRelationships(ModelBuilder modelBuilder)
    {
        // user
        modelBuilder.Entity<Card>()
            .HasOne(card => card.User)
            .WithOne(user => user.Card)
            .HasForeignKey<Card>(card => card.UserID);
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
            .HasForeignKey(account => account.BankID);
    }
}