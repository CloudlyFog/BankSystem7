﻿using BankSystem7.Models;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Configuration;

public class ModelConfiguration
{
    public virtual void Invoke(ModelBuilder modelBuilder)
    {
        ConfigureCardRelationships(modelBuilder);
        ConfigureCreditRelationships(modelBuilder);
        ConfigureBankAccountRelationships(modelBuilder);
        
        modelBuilder.Entity<User>().Ignore(user => user.Exception);
    }

    private static void ConfigureCreditRelationships(ModelBuilder modelBuilder)
    {
        // bank
        modelBuilder.Entity<Credit>()
            .HasOne(credit => credit.Bank)
            .WithMany(bank => bank.Credits)
            .HasForeignKey(credit => credit.BankID)
            .OnDelete(DeleteBehavior.Cascade);
        
        // user
        modelBuilder.Entity<Credit>()
            .HasOne(credit => credit.User)
            .WithOne(user => user.Credit)
            .HasForeignKey<Credit>(credit => credit.UserID)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureCardRelationships(ModelBuilder modelBuilder)
    {
        // user
        modelBuilder.Entity<Card>()
            .HasOne(card => card.User)
            .WithOne(user => user.Card)
            .HasForeignKey<Card>(card => card.UserID)
            .OnDelete(DeleteBehavior.Cascade);

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
            .HasForeignKey(account => account.BankID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}