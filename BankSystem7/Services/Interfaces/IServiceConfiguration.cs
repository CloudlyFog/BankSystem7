using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Repositories;

namespace BankSystem7.Services.Interfaces;

public interface IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit> : IDisposable
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    public BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankAccountRepository { get; protected internal set; }
    public BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankRepository { get; protected internal set; }
    public CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CardRepository { get; protected internal set; }
    public UserRepository<TUser, TCard, TBankAccount, TBank, TCredit>? UserRepository { get; protected internal set; }
    public CreditRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CreditRepository { get; protected internal set; }
    public LoggerRepository? LoggerRepository { get; protected internal set; }
    public OperationRepository? OperationRepository { get; protected internal set; }
    public ILogger? Logger { get; protected internal set; }
    public ConfigurationOptions? Options { get; protected internal set; }
}