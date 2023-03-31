using BankSystem7.AppContext;
using BankSystem7.Services.Repositories;
using Microsoft.AspNetCore.Http;

namespace BankSystem7.Services;

public class ServiceConfiguration
{
    public static string Connection { get; private set; } =
        @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=Test;
            Integrated Security=True;Persist Security Info=False;Pooling=False;
            MultipleActiveResultSets=False; Encrypt=False;TrustServerCertificate=False";

    public const string DefaultDataSource = "maxim";
    private const string DefaultDatabaseName = "Test";

    
    private bool _disposed;
    public BankAccountRepository? BankAccountRepository { get; set; }
    public BankRepository? BankRepository { get; set; }
    public CardRepository? CardRepository { get; set; }
    public UserRepository? UserRepository { get; set; }
    public CreditRepository? CreditRepository { get; set; }
    public static ConfigurationOptions Options { get; set; }

    private ServiceConfiguration()
    {
        BankAccountRepository = new BankAccountRepository(Connection);
        UserRepository = new UserRepository(BankAccountRepository);
        CardRepository = new CardRepository(BankAccountRepository, Connection);
        BankRepository = new BankRepository(Connection);
        CreditRepository = new CreditRepository(Connection);
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
        }

        BankAccountRepository = null;
        BankRepository = null;
        CardRepository = null;
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