using BankSystem7.BankAggregate;
using BankSystem7.BankAggregate.BankAccountAggregate;
using BankSystem7.BankAggregate.CardAggregate;
using BankSystem7.BankAggregate.CreditAggregate;
using BankSystem7.Configuration;
using BankSystem7.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.ApplicationAggregate.Data;

public class ApplicationContext : GenericDbContext
{
    protected internal DbSet<User> Users { get; set; } = null!;
    protected internal DbSet<Card> Cards { get; set; } = null!;
    protected internal DbSet<BankAccount> BankAccounts { get; set; } = null!;
    protected internal DbSet<Bank> Banks { get; set; } = null!;
    protected internal DbSet<Credit> Credits { get; set; } = null!;


    public ApplicationContext(ConfigurationOptions options) : base(options)
    {
    }

    public ApplicationContext(DbContextOptions dbContextOptions, ConfigurationOptions options) : base(dbContextOptions, options)
    {
    }
}