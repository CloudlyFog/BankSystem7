using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using BankSystem7.Services.Repositories;

namespace BankSystem7.Services.Configuration;

public class ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit> : IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    public static readonly IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit> Default =
        new ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>();

    private bool _disposed;

    public BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankAccountRepository { get; set; }

    public BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankRepository { get; set; }

    public CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CardRepository { get; set; }

    public UserRepository<TUser, TCard, TBankAccount, TBank, TCredit>? UserRepository { get; set; }

    public CreditRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CreditRepository { get; set; }

    public LoggerRepository? LoggerRepository { get; set; }

    public OperationRepository? OperationRepository { get; set; }

    public ILogger? Logger { get; set; }

    public ConfigurationOptions? Options { get; set; }

    internal static ConfigurationOptions? ServiceConfigurationOptions { get; set; }

    public void Dispose()
    {
        if (_disposed)
            return;

        BankAccountRepository?.Dispose();
        BankRepository?.Dispose();
        CardRepository?.Dispose();
        UserRepository?.Dispose();
        CreditRepository?.Dispose();
        OperationRepository?.Dispose();

        _disposed = true;
    }
}