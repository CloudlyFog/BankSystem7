using Microsoft.EntityFrameworkCore;
using Standart7.Models;

namespace BankSystem7.Services;

public sealed class DatabaseHandler : DbContext
{
    private static string _connection = string.Empty;
    
    public static User[]? SetUsers { get; set; } = null;
    public static Bank[]? SetBanks { get; set; } = null;
    public static Operation[]? SetOperations { get; set; } = null;
    public static BankAccount[]? SetBankAccounts { get; set; } = null;
    public static Credit[]? SetCredits { get; set; } = null;
    public static Card[]? SetCards { get; set; } = null;
    
    public DatabaseHandler(string connection, bool deleteDatabase)
    {
        _connection = connection;
        if (deleteDatabase)
            Database.EnsureDeleted();
        Database.EnsureCreated();
    }
    
    internal DbSet<User> Users { get; set; } = null!;
    internal DbSet<Bank> Banks { get; set; } = null!;
    internal DbSet<Operation> Operations { get; set; } = null!;
    internal DbSet<BankAccount> BankAccounts { get; set; } = null!;
    internal DbSet<Credit> Credits { get; set; } = null!;
    internal DbSet<Card> Cards { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_connection.Equals(string.Empty))
        {
            throw new Exception(
                "Connection string is empty. Please specify connection string in constructor of class.");
        }
        optionsBuilder.UseSqlServer(_connection);
        optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (SetUsers is not null)
            modelBuilder.Entity<User>().HasData(SetUsers);
        
        if (SetBanks is not null)
            modelBuilder.Entity<Bank>().HasData(SetBanks);
        
        if (SetOperations is not null)
            modelBuilder.Entity<Operation>().HasData(SetOperations);
        
        if (SetBankAccounts is not null)
            modelBuilder.Entity<BankAccount>().HasData(SetBankAccounts);
        
        if (SetCredits is not null)
            modelBuilder.Entity<Credit>().HasData(SetCredits);
        
        if (SetCards is not null)
            modelBuilder.Entity<Card>().HasData(SetCards);
        
        base.OnModelCreating(modelBuilder);
    }
}