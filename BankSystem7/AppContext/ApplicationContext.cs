using BankSystem7.Models;
using BankSystem7.Services;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.AppContext;

public class ApplicationContext<TUser> : DbContext where TUser : User
{
    public static bool EnsureCreated { get; set; } = true;
    public static bool EnsureDeleted { get; set; }
    protected internal DbSet<TUser> Users { get; set; } = null!;
    protected internal DbSet<BankAccount> BankAccounts { get; set; } = null!;
    protected internal DbSet<Bank> Banks { get; set; } = null!;
    protected internal DbSet<Card> Cards { get; set; } = null!;
    protected internal DbSet<Credit> Credits { get; set; } = null!;
    
    public ApplicationContext()
    {
        DatabaseHandle();
    }

    public ApplicationContext(string connection)
    {
        ServiceConfiguration<TUser>.SetConnection(connection);
        DatabaseHandle();
    }

    public ApplicationContext(BankAccountRepository<TUser> bankAccountRepository)
    {
        DatabaseHandle();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseSqlServer(ServiceConfiguration<TUser>.Connection);
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
        if (BankServicesOptions<TUser>.Ensured)
            return;
        if (EnsureDeleted)
            Database.EnsureDeleted();
        if (EnsureCreated)
            Database.EnsureCreated();
        BankServicesOptions<TUser>.Ensured = true;
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