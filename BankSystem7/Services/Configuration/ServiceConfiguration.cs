using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using BankSystem7.Services.Repositories;
using Microsoft.AspNetCore.Http;

namespace BankSystem7.Services.Configuration;

public class ServiceConfiguration<TUser> where TUser : User
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
        BankAccountRepository = new BankAccountRepository<TUser>(Connection);
        UserRepository = new UserRepository<TUser>(BankAccountRepository);
        CardRepository = new CardRepository<TUser>(BankAccountRepository);
        BankRepository = new BankRepository<TUser>(Connection);
        CreditRepository = new CreditRepository<TUser>(Connection);
        if (Options.LoggerOptions.IsEnabled)
        {
            LoggerRepository = new LoggerRepository(Options.LoggerOptions);
            Logger = new Logger<TUser>(LoggerRepository, Options.LoggerOptions);
        }
        OperationRepository = new OperationRepository<TUser>(Logger, Options.OperationOptions);
    }
    
    public BankAccountRepository<TUser>? BankAccountRepository { get; protected internal  set; }
    public BankRepository<TUser>? BankRepository { get; protected internal set; }
    public CardRepository<TUser>? CardRepository { get; protected internal set; }
    public UserRepository<TUser>? UserRepository { get; protected internal set; }
    public CreditRepository<TUser>? CreditRepository { get; protected internal set; }
    public LoggerRepository? LoggerRepository { get; protected internal set; }
    public OperationRepository<TUser>? OperationRepository { get; protected internal set; }
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
            BankServicesOptions<TUser>.Connection = Connection;
            return;
        }

        if (databaseName is null || dataSource is null)
        {
            BankServicesOptions<TUser>.Connection = Connection;
            return;
        }
        
        Connection = @$"Server=localhost\\SQLEXPRESS;Data Source={dataSource};Initial Catalog={databaseName};
            Integrated Security=True;Persist Security Info=False;Pooling=False;
            MultipleActiveResultSets=False; Encrypt=False;TrustServerCertificate=False";
        BankServicesOptions<TUser>.Connection = Connection;
    }

    public static ServiceConfiguration<TUser> CreateInstance(ConfigurationOptions options)
    {
        Options = options;
        SetConnection(options.Connection, options.DatabaseName);
        ApplicationContext<TUser>.EnsureDeleted = BankServicesOptions<TUser>.EnsureDeleted = options.EnsureDeleted;
        ApplicationContext<TUser>.EnsureCreated = BankServicesOptions<TUser>.EnsureCreated = options.EnsureCreated;

        BankServicesOptions<TUser>.ServiceConfiguration = new ServiceConfiguration<TUser>();
        return BankServicesOptions<TUser>.ServiceConfiguration;
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