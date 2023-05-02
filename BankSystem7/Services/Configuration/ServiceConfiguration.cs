using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using BankSystem7.Services.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Configuration;

public class ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit> : IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>
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

    public BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankAccountRepository { get; }
    public BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankRepository { get; }
    public CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CardRepository { get; }
    public UserRepository<TUser, TCard, TBankAccount, TBank, TCredit>? UserRepository { get; }
    public CreditRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CreditRepository { get; }
    public LoggerRepository? LoggerRepository { get; }
    public OperationRepository<TUser, TCard, TBankAccount, TBank, TCredit>? OperationRepository { get; }
    public ILogger? Logger { get; }
    protected internal static ConfigurationOptions? Options { get; set; }

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
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.EnsureDeleted = options.EnsureDeleted;
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.EnsureCreated = options.EnsureCreated;

        InitDbContexts(options?.Contexts);

        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration = new ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>();
        return BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration;
    }

    private static void InitDbContexts(Dictionary<DbContext, object> contexts)
    {
        if (contexts is null || contexts.Keys.Count == 0)
            return;

        if (BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Ensured)
            return;

        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.InitializeAccess = true;

        foreach (var context in contexts)
            context.Key.GetType()?.GetConstructor(new Type[] { context.Value.GetType() })?.Invoke(new object[] { context.Value });
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