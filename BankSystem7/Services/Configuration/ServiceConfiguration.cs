using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using BankSystem7.Services.Interfaces.Base;
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
    
    public IUserRepository<TUser>? UserRepository { get; set; }

    public ICardRepository<TCard>? CardRepository { get; set; }
    public IBankAccountRepository<TBankAccount>? BankAccountRepository { get; set; }

    public IBankRepository<TBank>? BankRepository { get; set; }

    public ICreditRepository<TCredit>? CreditRepository { get; set; }

    public LoggerRepository? LoggerRepository { get; set; }

    public IOperationRepository? OperationRepository { get; set; }

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