using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using BankSystem7.Services.Repositories;
using Microsoft.AspNetCore.Http;

namespace BankSystem7.Services.Configuration;

public class ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    public static string Connection { get; private set; } =
        @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=Test;
            Integrated Security=True;Persist Security Info=False;Pooling=False;
            MultipleActiveResultSets=False; Encrypt=False;TrustServerCertificate=False";

    public const string DefaultDataSource = "maxim";
    private const string DefaultDatabaseName = "Test";

    private bool _disposed;

    private ServiceConfiguration()
    {
        BankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(Connection);
        UserRepository = new UserRepository<TUser, TCard, TBankAccount, TBank, TCredit>(BankAccountRepository);
        CardRepository = new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(BankAccountRepository);
        BankRepository = new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(Connection);
        CreditRepository = new CreditRepository<TUser, TCard, TBankAccount, TBank, TCredit>(Connection);
        if (Options.LoggerOptions.IsEnabled)
        {
            LoggerRepository = new LoggerRepository(Options.LoggerOptions);
            Logger = new Logger<TUser, TCard, TBankAccount, TBank, TCredit>(LoggerRepository, Options.LoggerOptions);
        }
        OperationRepository = new OperationRepository<TUser, TCard, TBankAccount, TBank, TCredit>(Options.OperationOptions);
    }

    public BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankAccountRepository { get; protected internal set; }
    public BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankRepository { get; protected internal set; }
    public CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CardRepository { get; protected internal set; }
    public UserRepository<TUser, TCard, TBankAccount, TBank, TCredit>? UserRepository { get; protected internal set; }
    public CreditRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CreditRepository { get; protected internal set; }
    public LoggerRepository? LoggerRepository { get; protected internal set; }
    public OperationRepository<TUser, TCard, TBankAccount, TBank, TCredit>? OperationRepository { get; protected internal set; }
    public ILogger? Logger { get; protected internal set; }
    public static ConfigurationOptions Options { get; protected internal set; }

    public ServiceConfiguration(RequestDelegate next, ConfigurationOptions options)
    {
        CreateInstance(options);
    }

    public static void SetConnection(string? connection = null, string? databaseName = DefaultDatabaseName, string? dataSource = DefaultDataSource)
    {
        if (connection is not null && connection != string.Empty)
        {
            Connection = connection;
            BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection = Connection;
            return;
        }

        if (databaseName is null || dataSource is null)
        {
            BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection = Connection;
            return;
        }

        Connection = @$"Server=localhost\\SQLEXPRESS;Data Source={dataSource};Initial Catalog={databaseName};
            Integrated Security=True;Persist Security Info=False;Pooling=False;
            MultipleActiveResultSets=False; Encrypt=False;TrustServerCertificate=False";
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection = Connection;
    }

    public static ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit> CreateInstance(ConfigurationOptions options)
    {
        Options = options;
        SetConnection(options.Connection, options.DatabaseName);
        ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>.EnsureDeleted = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.EnsureDeleted = options.EnsureDeleted;
        ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>.EnsureCreated = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.EnsureCreated = options.EnsureCreated;

        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration = new ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>();
        return BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration;
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

        BankAccountRepository = null;
        BankRepository = null;
        CardRepository = null;
        UserRepository = null;
        CreditRepository = null;
        LoggerRepository = null;
        Logger = null;
        OperationRepository = null;
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