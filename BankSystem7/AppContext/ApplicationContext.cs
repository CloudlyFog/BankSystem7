using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Standart7.Models;

namespace BankSystem7.AppContext;

public class ApplicationContext : DbContext
{
    public const string Connection =
        @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=NationBankUnion;Integrated Security=True;
            Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;
            Encrypt=False;TrustServerCertificate=False";

    protected internal DbSet<User> Users { get; set; } = null!;
    protected internal DbSet<BankAccount> BankAccounts { get; set; } = null!;
    protected internal DbSet<Operation> Operations { get; set; } = null!;
    protected internal DbSet<Bank> Banks { get; set; } = null!;
    protected internal DbSet<Card> Cards { get; set; } = null!;


    public ApplicationContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseSqlServer(Connection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
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
        //
        modelBuilder.Entity<User>()
            .Ignore(nameof(Exception))
            .Ignore(nameof(Warning))
            .Ignore(nameof(User.Initiable));
        modelBuilder.Entity<User>().HasData(users);
        //
        modelBuilder.Entity<BankAccount>().HasData(bankAccounts);
        modelBuilder.Entity<Bank>().HasData(banks);
        base.OnModelCreating(modelBuilder);
    }
    public string HashPassword(string password)
    {
        // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
        var salt = new byte[128 / 8];

        // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
    }
}