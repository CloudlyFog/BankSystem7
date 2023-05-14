using BankSystem7.Models;
using BankSystem7.Services.Repositories;

namespace BankSystem7.Services.Interfaces;

public interface IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit> : IDisposable
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    public BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankAccountRepository { get; }
    public BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankRepository { get; }
    public CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CardRepository { get; }
    public UserRepository<TUser, TCard, TBankAccount, TBank, TCredit>? UserRepository { get; }
    public CreditRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CreditRepository { get; }
    public LoggerRepository? LoggerRepository { get; }
    public OperationRepository? OperationRepository { get; }
    public ILogger? Logger { get; }
}