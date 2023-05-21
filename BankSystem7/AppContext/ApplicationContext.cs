using BankSystem7.Models;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.AppContext;

public class ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> : GenericDbContext
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    protected internal DbSet<TUser> Users { get; set; } = null!;
    protected internal DbSet<TCard> Cards { get; set; } = null!;
    protected internal DbSet<TBankAccount> BankAccounts { get; set; } = null!;
    protected internal DbSet<TBank> Banks { get; set; } = null!;
    protected internal DbSet<TCredit> Credits { get; set; } = null!;

    public ApplicationContext() : base()
    {
    }

    public ApplicationContext(string connection) : base(connection)
    {
    }

    internal ExceptionModel AvoidDuplication(Bank item)
    {
        foreach (var bankAccount in item.BankAccounts)
            Entry(bankAccount).State = EntityState.Unchanged;

        foreach (var credit in item.Credits)
            Entry(credit).State = EntityState.Unchanged;

        return ExceptionModel.Ok;
    }
}