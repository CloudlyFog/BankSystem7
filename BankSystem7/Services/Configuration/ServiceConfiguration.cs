using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using BankSystem7.Services.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Configuration;

public class ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit> : DbContextInitializer, IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private bool _disposed;

    private ServiceConfiguration()
    {
        BankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
        UserRepository = new UserRepository<TUser, TCard, TBankAccount, TBank, TCredit>(BankAccountRepository);
        CardRepository = new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(BankAccountRepository);
        BankRepository = new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
        CreditRepository = new CreditRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
        if (Options.LoggerOptions.IsEnabled)
        {
            LoggerRepository = new LoggerRepository(Options.LoggerOptions);
            Logger = new Logger<TUser, TCard, TBankAccount, TBank, TCredit>(LoggerRepository, Options.LoggerOptions);
        }
        OperationRepository = new OperationRepository(Options.OperationOptions);
    }

    public BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankAccountRepository { get; }
    public BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankRepository { get; }
    public CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CardRepository { get; }
    public UserRepository<TUser, TCard, TBankAccount, TBank, TCredit>? UserRepository { get; }
    public CreditRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CreditRepository { get; }
    public LoggerRepository? LoggerRepository { get; }
    public OperationRepository? OperationRepository { get; }
    public ILogger? Logger { get; }
    protected internal static ConfigurationOptions? Options { get; set; }

    public static ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit> CreateInstance(ConfigurationOptions options)
    {
        Options = options;
        ServicesSettings.SetConnection(Options.ConnectionConfiguration.DatabaseManagementSystemType, Options.ConnectionConfiguration, Options.Credentials);
        ServicesSettings.EnsureDeleted = options.ConnectionConfiguration.EnsureDeleted;
        ServicesSettings.EnsureCreated = options.ConnectionConfiguration.EnsureCreated;
        ServicesSettings.InitializeAccess = true;
        ServicesSettings.DatabaseManagementSystemType = options.ConnectionConfiguration.DatabaseManagementSystemType;

        InitDbContexts(options?.Contexts);

        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration = new ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>();
        return BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration;
    }

    private static void InitDbContexts(Dictionary<DbContext, ModelConfiguration>? contexts)
    {
        if (contexts is null || contexts.Keys.Count == 0)
            return;

        if (ServicesSettings.Ensured)
            return;

        InitializeDbContexts(Options.Contexts);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing)
        {
            BankAccountRepository?.Dispose();
            BankRepository?.Dispose();
            CardRepository?.Dispose();
            UserRepository?.Dispose();
            CreditRepository?.Dispose();
            OperationRepository?.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ServiceConfiguration()
    {
        Dispose(false);
    }
}