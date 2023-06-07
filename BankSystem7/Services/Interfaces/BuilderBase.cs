using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Repositories;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Threading.Tasks;

namespace BankSystem7.Services.Interfaces;

public abstract class BuilderBase<TUser, TCard, TBankAccount, TBank, TCredit>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private bool _disposed;

    protected BuilderBase()
    {
        ServiceConfiguration = new ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>();
    }

    public BuilderSettings BuilderSettings { get; set; } = new();
    protected IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit> ServiceConfiguration { get; }

    public abstract IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>? Build();
    public abstract void BuildBankAccountRepository();
    public abstract void BuildUserRepository();
    public abstract void BuildCardRepository();
    public abstract void BuildBankRepository();
    public abstract void BuildCreditRepository();
    public abstract void BuildLoggerRepository();
    public abstract void BuildOperationRepository();
    public abstract void BuildLogger();
    public void Dispose()
    {
        if (_disposed)
            return;

        ServiceConfiguration?.Dispose();

        _disposed = true;
    }
}
