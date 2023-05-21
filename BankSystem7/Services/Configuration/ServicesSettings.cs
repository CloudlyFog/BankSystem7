using System.Text;
using BankSystem7.Models.Connection;
using BankSystem7.Models.Credentials;

namespace BankSystem7.Services.Configuration;

internal static class ServicesSettings
{
    public const string DefaultMicrosoftSqlServerConnection = @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=Test;
            Integrated Security=True;Persist Security Info=False;Pooling=False;
            MultipleActiveResultSets=False; Encrypt=False;TrustServerCertificate=False";

    public const string DefaultDataSource = "maxim";
    public const string DefaultDatabaseName = "Test";
    public const string DefaultHost = "localhost";
    public const string DefaultPort = "5432";
    public const string DefaultServer = "maxim";
    public static bool EnsureDeleted { get; set; }
    public static bool EnsureCreated { get; set; } = true;
    public static string? Connection { get; private set; }
    public static bool Ensured { get; set; }
    public static bool InitializeAccess { get; set; }
    public static DatabaseManagementSystemType DatabaseManagementSystemType { get; set; } 

    public static void SetConnection(string? connection = null, string? databaseName = DefaultDatabaseName, string? dataSource = DefaultDataSource)
    {
        if (connection is not null && connection != string.Empty)
        {
            Connection = connection;
            return;
        }

        if (databaseName is null || dataSource is null)
        {
            Connection = DefaultMicrosoftSqlServerConnection;
            return;
        }

        Connection = GetConnectionByDbmsType(DatabaseManagementSystemType, databaseName, dataSource);
    }

    public static void SetConnection(DatabaseManagementSystemType type,
        ConnectionConfigurationBase connectionConfiguration, CredentialsBase credentials)
    {
        if (connectionConfiguration.Connection is not null && connectionConfiguration.Connection != string.Empty)
        {
            Connection = connectionConfiguration.Connection;
            return;
        }

        if (connectionConfiguration.DatabaseName is null)
        {
            Connection = DefaultMicrosoftSqlServerConnection;
            return;
        }

        Connection = GetConnectionByDbmsType(type, connectionConfiguration, credentials);
    }

    private static string GetConnectionByDbmsType(DatabaseManagementSystemType type, string? databaseName = DefaultDatabaseName, string? dataSource = DefaultDataSource)
    {
        var connectionString = type switch
        {
            DatabaseManagementSystemType.MicrosoftSqlServer =>
                @$"Server=localhost\\SQLEXPRESS;Data Source={dataSource};Initial Catalog={databaseName};
                Integrated Security=True;Persist Security Info=False;Pooling=False;
                MultipleActiveResultSets=False; Encrypt=False;TrustServerCertificate=False",
            DatabaseManagementSystemType.PostgreSql => $"postgres://YourUserName:YourPassword@{dataSource}:5433/{databaseName}",
            DatabaseManagementSystemType.MySql => "",
            _ => DefaultMicrosoftSqlServerConnection
        };

        return connectionString;
    }

    private static string GetConnectionByDbmsType(DatabaseManagementSystemType type,
        ConnectionConfigurationBase connectionConfiguration, CredentialsBase credentials)
    {
        switch (type)
        {
            case DatabaseManagementSystemType.MicrosoftSqlServer:
            {
                var microsoftCredentials = (MicrosoftCredentials)credentials;
                var microsoftConnectionConfiguration = (MicrosoftConnectionConfiguration)connectionConfiguration;
                var connection = new StringBuilder();
                connection.Append(
                    $"Server={microsoftConnectionConfiguration.Server};Database={microsoftConnectionConfiguration.DatabaseName};");
                connection.Append($"Integrated Security={microsoftConnectionConfiguration.IntegratedSecurity};");
                connection.Append($"Persist Security Info={microsoftConnectionConfiguration.PersistSecurityInfo};Pooling={microsoftConnectionConfiguration.Pooling};");
                connection.Append($"Encrypt={microsoftConnectionConfiguration.Encrypt};TrustServerCertificate={microsoftConnectionConfiguration.TrustServerCertificate};");
                connection.Append($"User Id={microsoftCredentials.Username};Password={microsoftCredentials.Password};");
                Connection = connection.ToString();
                break;
            }
            case DatabaseManagementSystemType.PostgreSql:
            {
                var npgsqlCredentials = (NpgsqlCredentials)credentials;
                var npgsqlConnectionConfiguration = (NpgsqlConnectionConfiguration)connectionConfiguration;
                Connection =
                    $"Server={npgsqlConnectionConfiguration.Host};Port={npgsqlConnectionConfiguration.Port};Database={npgsqlConnectionConfiguration.DatabaseName};User Id={npgsqlCredentials.Username};Password={npgsqlCredentials.Password}";
                break;
            }
            case DatabaseManagementSystemType.MySql:
            {
                Connection = $"";
                break;
            }
            default:
            {
                Connection = DefaultMicrosoftSqlServerConnection;
                break;
            }
        }

        return Connection;
    }
}