using BankSystem7.Models;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services;

public sealed class ModelConfiguration
{
    public static void Invoke(ModelBuilder modelBuilder)
    {
        ConfigureCreditRelationships(modelBuilder);
        ConfigureUserRelationships(modelBuilder);
        ConfigureBankAccount(modelBuilder);
    }
    private static void ConfigureCreditRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Credit>()
            .HasOne(credit => credit.Bank)
            .WithMany(bank => bank.Credits)
            .HasForeignKey(credit => credit.BankID);
    }

    private static void ConfigureUserRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Credit>()
            .HasOne(credit => credit.User)
            .WithOne(user => user.Credit)
            .HasForeignKey<Credit>(credit => credit.UserID)
            .OnDelete(DeleteBehavior.NoAction);
        
        modelBuilder.Entity<Card>()
            .HasOne(card => card.User)
            .WithOne(user => user.Card)
            .HasForeignKey<Card>(card => card.UserID)
            .OnDelete(DeleteBehavior.NoAction);
        
        modelBuilder.Entity<User>()
            .Ignore(nameof(Exception))
            .Ignore(nameof(Warning));
    }

    private static void ConfigureBankAccount(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankAccount>()
            .HasOne(account => account.Card)
            .WithOne(card => card.BankAccount)
            .HasForeignKey<Card>(card => card.BankAccountID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BankAccount>()
            .HasOne(account => account.Bank)
            .WithMany(bank => bank.BankAccounts)
            .HasForeignKey(account => account.BankID);
    }

    private static void ConfigureBankRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bank>()
            .HasMany(bank => bank.Credits)
            .WithOne(credit => credit.Bank)
            .HasForeignKey(credit => credit.BankID);

        modelBuilder.Entity<Bank>()
            .HasMany(bank => bank.BankAccounts)
            .WithOne(account => account.Bank)
            .HasForeignKey(account => account.BankID);
    }
}