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
    public BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankAccountRepository { get; internal protected set; }
    public BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankRepository { get; internal protected set; }
    public CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CardRepository { get; internal protected set; }
    public UserRepository<TUser, TCard, TBankAccount, TBank, TCredit>? UserRepository { get; internal protected set; }
    public CreditRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CreditRepository { get; internal protected set; }
    public LoggerRepository? LoggerRepository { get; }
    public OperationRepository? OperationRepository { get; }
    public ILogger? Logger { get; }
    protected internal static ConfigurationOptions? Options { get; set; }
}