using Microsoft.EntityFrameworkCore;
using Standart7.Models;
using Standart7.Services;

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
        if (EnsureDeleted)
            Database.EnsureDeleted();
        if (EnsureCreated)
            Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ServiceConfiguration.SetConnection(databaseName: "CabManagementSystemReborn");
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
                BankAccountID = new Guid("e26ccdd6-4e91-4611-93be-baefeb7f5047"),
                BankID = new Guid("bed62930-9356-477a-bed5-b84d59336122"),
                CardID = new Guid("8b38f63b-0433-485c-9a6c-dff20d808a14"),
                BankAccountAmount = 18_000,
                Age = 23
            }
        };
        var banks = new List<Bank>()
        {
            new()
            {
                ID = new Guid("ae59b1df-089d-4823-a32a-41a44f878b4b"),
                BankID = new Guid("bed62930-9356-477a-bed5-b84d59336122"),
                BankName = "Tinkoff",
                AccountAmount = 234523450
            },
            new()
            {
                ID = new Guid("c2c4fc26-e503-4d48-8a24-ad9233e0e603"),
                BankID = new Guid("e4c18139-f2c8-4a4b-a8b8-cf0d230b37fa"),
                BankName = "SberBank",
                AccountAmount = 1043200000
            },
            new()
            {
                ID = new Guid("335ba509-2994-4068-9a50-f703490891ba"),
                BankID = new Guid("b56c8051-6eee-4441-a7de-7cb4789de362"),
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
                CardID = new Guid("5c7b3546-6450-4962-a208-1e656bddcc64"),
                BankAccountAmount = 100_000,
                PhoneNumber = "9431234423",
                AccountType = AccountType.User
            },
            new()
            {
                ID = new Guid("e26ccdd6-4e91-4611-93be-baefeb7f5047"),
                BankID = new Guid("bed62930-9356-477a-bed5-b84d59336122"),
                UserID = new Guid("b5616fe2-0517-4a2f-aff3-e409fdbcafa0"),
                CardID = new Guid("8b38f63b-0433-485c-9a6c-dff20d808a14"),
                BankAccountAmount = 10_000,
                PhoneNumber = "9431134423",
                AccountType = AccountType.User
            }
        };
        modelBuilder.Entity<User>().HasData(users);
        modelBuilder.Entity<BankAccount>().HasData(bankAccounts);
        modelBuilder.Entity<Bank>().HasData(banks);
    }
}