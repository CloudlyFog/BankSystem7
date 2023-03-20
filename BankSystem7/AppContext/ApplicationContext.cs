using BankSystem7.Models;
using BankSystem7.Services;
using BankSystem7.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.AppContext;

public class ApplicationContext : DbContext
{
    public static bool EnsureDeleted { get; set; }
    public static bool EnsureCreated { get; set; } = true;
    protected DbSet<User> Users { get; set; } = null!;
    protected internal DbSet<BankAccount> BankAccounts { get; set; } = null!;
    protected internal DbSet<Operation> Operations { get; set; } = null!;
    protected internal DbSet<Bank> Banks { get; set; } = null!;
    protected internal DbSet<Card> Cards { get; set; } = null!;
    protected internal DbSet<Credit> Credits { get; set; } = null!;


    public ApplicationContext()
    {
        DatabaseHandle();
    }

    public ApplicationContext(string connection)
    {
        ServiceConfiguration.SetConnection(connection);
        DatabaseHandle();
    }

    public ApplicationContext(BankAccountRepository bankAccountRepository)
    {
        DatabaseHandle();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseSqlServer(ServiceConfiguration.Connection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ModelConfiguration.Invoke(modelBuilder);
        
        base.OnModelCreating(modelBuilder);
    }

    private void FillDatabase(ModelBuilder modelBuilder)
    {
        var users = new List<User>()
        {
            new()
            {
                ID = new Guid("b5616fe2-0517-4a2f-aff3-e409fdbcafa0"),
                Name = "maxim lebedev",
                Email = "cloudyfg@gmail.com",
                Password = "ZWMrBybtiDYhRBOSVdG0t2Y+dMPtYbxfcP171UTazXE=",
                Authenticated = false,
                PhoneNumber = "79431134423",
                BankID = new Guid("bed62930-9356-477a-bed5-b84d59336122"),
                Age = 23
            }
        };
        var banks = new List<Bank>()
        {
            new()
            {
                ID = new Guid("ae59b1df-089d-4823-a32a-41a44f878b4b"),
                BankName = "Tinkoff",
                AccountAmount = 234523450
            },
            new()
            {
                ID = new Guid("c2c4fc26-e503-4d48-8a24-ad9233e0e603"),
                BankName = "SberBank",
                AccountAmount = 1043200000
            },
            new()
            {
                ID = new Guid("335ba509-2994-4068-9a50-f703490891ba"),
                BankName = "PochtaBank",
                AccountAmount = 100650000
            },
        };
        var bankAccounts = new List<BankAccount>()
        {
            new()
            {
                ID = new Guid("a39a83d4-4947-4d63-8911-34346d9d5425"),
                BankID = new Guid("bed62930-9356-477a-bed5-b84d59336122"),
                UserID = new Guid("b5616fe2-0517-4a2f-aff3-e409fdbcafa0"),
                BankAccountAmount = 100_000,
                PhoneNumber = "9431234423",
                AccountType = AccountType.User
            },
            new()
            {
                ID = new Guid("e26ccdd6-4e91-4611-93be-baefeb7f5047"),
                BankID = new Guid("bed62930-9356-477a-bed5-b84d59336122"),
                UserID = new Guid("b5616fe2-0517-4a2f-aff3-e409fdbcafa0"),
                BankAccountAmount = 10_000,
                PhoneNumber = "9431134423",
                AccountType = AccountType.User
            }
        };
        modelBuilder.Entity<User>().HasData(users);
        modelBuilder.Entity<BankAccount>().HasData(bankAccounts);
        modelBuilder.Entity<Bank>().HasData(banks);
    }

    private void DatabaseHandle()
    {
        if (EnsureDeleted)
            Database.EnsureDeleted();
        if (EnsureCreated)
            Database.EnsureCreated();
    }
}