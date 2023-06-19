using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Interfaces.Base;
using BankSystem7.Services.Repositories;

namespace BankSystem7.Services.Interfaces;

public interface IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit> : IDisposable
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    public IUserRepository<TUser>? UserRepository { get; protected internal set; }

    public ICardRepository<TCard>? CardRepository { get; protected internal set; }
    public IBankAccountRepository<TBankAccount>? BankAccountRepository { get; protected internal set; }

    public IBankRepository<TBank>? BankRepository { get; protected internal set; }

    public ICreditRepository<TCredit>? CreditRepository { get; protected internal set; }

    public LoggerRepository? LoggerRepository { get; protected internal set; }

    public IOperationRepository? OperationRepository { get; protected internal set; }
    public ILogger? Logger { get; protected internal set; }
    public ConfigurationOptions? Options { get; protected internal set; }
}