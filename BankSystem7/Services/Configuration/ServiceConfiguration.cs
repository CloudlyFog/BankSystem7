using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using BankSystem7.Services.Repositories;
using Microsoft.AspNetCore.Http;

namespace BankSystem7.Services.Configuration;

public class ServiceConfiguration
{
    public static string Connection { get; private set; } =
        @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=Test;
            Integrated Security=True;Persist Security Info=False;Pooling=False;
            MultipleActiveResultSets=False; Encrypt=False;TrustServerCertificate=False";

    public const string DefaultDataSource = "maxim";
    private const string DefaultDatabaseName = "Test";

    
    private bool _disposed;
    public BankAccountRepository? BankAccountRepository { get; protected internal  set; }
    public BankRepository? BankRepository { get; protected internal set; }
    public CardRepository? CardRepository { get; protected internal set; }
    public UserRepository? UserRepository { get; protected internal set; }
    public CreditRepository? CreditRepository { get; protected internal set; }
    public LoggerRepository? LoggerRepository { get; protected internal set; }
    public OperationRepository? OperationRepository { get; protected internal set; }
    public ILogger? Logger { get; protected internal set; }
    public static ConfigurationOptions Options { get; protected internal set; }

    private ServiceConfiguration()
    {
        BankAccountRepository = new BankAccountRepository(Connection);
        UserRepository = new UserRepository(BankAccountRepository);
        CardRepository = new CardRepository(BankAccountRepository);
        BankRepository = new BankRepository(Connection);
        CreditRepository = new CreditRepository(Connection);
        LoggerRepository = new LoggerRepository(Options.LoggerOptions);
        Logger = new Logger(LoggerRepository, Options.LoggerOptions);
        OperationRepository = new OperationRepository(Logger);
    }

    public ServiceConfiguration(RequestDelegate next, ConfigurationOptions options)
    {
        CreateInstance(options);
    }

    public static void SetConnection(string? connection = null, string? databaseName = DefaultDatabaseName, string? dataSource = DefaultDataSource)
    {
        if (connection is not null && connection != string.Empty)
        {
            Connection = connection;
            BankServicesOptions.Connection = Connection;
            return;
        }

        if (databaseName is null || dataSource is null)
        {
            BankServicesOptions.Connection = Connection;
            return;
        }
        
        Connection = @$"Server=localhost\\SQLEXPRESS;Data Source={dataSource};Initial Catalog={databaseName};
            Integrated Security=True;Persist Security Info=False;Pooling=False;
            MultipleActiveResultSets=False; Encrypt=False;TrustServerCertificate=False";
        BankServicesOptions.Connection = Connection;
    }

    public static ServiceConfiguration CreateInstance()
    {
        BankServicesOptions.ServiceConfiguration = new ServiceConfiguration();
        return BankServicesOptions.ServiceConfiguration;
    }

    public static ServiceConfiguration CreateInstance(string? connection = null, string? databaseName = DefaultDatabaseName)
    {
        SetConnection(connection, databaseName);
        BankServicesOptions.ServiceConfiguration = new ServiceConfiguration();
        return BankServicesOptions.ServiceConfiguration;
    }

    public static ServiceConfiguration CreateInstance(ConfigurationOptions options)
    {
        Options = options;
        SetConnection(options.Connection, options.DatabaseName);
        ApplicationContext.EnsureDeleted = options.EnsureDeleted;
        ApplicationContext.EnsureCreated = options.EnsureCreated;

        BankServicesOptions.ServiceConfiguration = new ServiceConfiguration();
        return BankServicesOptions.ServiceConfiguration;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            BankAccountRepository?.Dispose();
            BankRepository?.Dispose();
            CardRepository?.Dispose();
            UserRepository?.Dispose();
            CreditRepository?.Dispose();
            LoggerRepository.Dispose();
        }

        BankAccountRepository = null;
        BankRepository = null;
        CardRepository = null;
        UserRepository = null;
        CreditRepository = null;
        LoggerRepository = null;
        Logger = null;
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