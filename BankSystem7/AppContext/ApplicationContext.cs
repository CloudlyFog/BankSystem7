using BankSystem7.Models;
using BankSystem7.Services;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.AppContext;

public class ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> : DbContext
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    public static bool EnsureCreated { get; set; } = true;
    public static bool EnsureDeleted { get; set; }
    protected internal DbSet<TUser> Users { get; set; } = null!;
    protected internal DbSet<TCard> Cards { get; set; } = null!;
    protected internal DbSet<TBankAccount> BankAccounts { get; set; } = null!;
    protected internal DbSet<TBank> Banks { get; set; } = null!;
    protected internal DbSet<TCredit> Credits { get; set; } = null!;

    public ApplicationContext()
    {
        DatabaseHandle();
    }

    public ApplicationContext(string connection)
    {
        ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>.SetConnection(connection);
        DatabaseHandle();
    }

    public ApplicationContext(BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit> bankAccountRepository)
    {
        DatabaseHandle();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder
            .UseSqlServer(ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
        //optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new ModelConfiguration<TUser>().Invoke(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// handle creating and deleting database
    /// </summary>
    private void DatabaseHandle()
    {
        if (BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Ensured)
            return;
        if (EnsureDeleted)
            Database.EnsureDeleted();
        if (EnsureCreated)
            Database.EnsureCreated();
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Ensured = true;
    }

    internal ExceptionModel AvoidDuplication(Bank item)
    {
        foreach (var bankAccount in item.BankAccounts)
            Entry(bankAccount).State = EntityState.Unchanged;

        foreach (var credit in item.Credits)
            Entry(credit).State = EntityState.Unchanged;

        return ExceptionModel.Successfully;
    }
}