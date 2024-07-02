using System.Text;
using BankSystem7.ApplicationAggregate.Connection;
using BankSystem7.ApplicationAggregate.Credentials;
using BankSystem7.Configuration;

namespace BankSystem7.ApplicationAggregate.Helpers;

internal static class DatabaseConnectionStringHelper
{
    public static string GetConnectionString(DatabaseManagementSystemType type,
        ConnectionConfigurationBase connectionConfiguration, CredentialsBase credentials)
    {
        if (string.IsNullOrWhiteSpace(connectionConfiguration?.ConnectionString))
            return connectionConfiguration.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionConfiguration?.DatabaseName))
            return string.Empty;

        if (string.IsNullOrWhiteSpace(credentials?.Username) || string.IsNullOrWhiteSpace(credentials?.Password))
            return string.Empty;

        return GetConnectionByDbmsType(type, connectionConfiguration, credentials);
    }

    private static string GetConnectionByDbmsType(DatabaseManagementSystemType type,
        ConnectionConfigurationBase connectionConfiguration, CredentialsBase credentials)
    {
        return type switch
        {
            DatabaseManagementSystemType.MicrosoftSqlServer => GetMicrosoftSqlServerConnectionString(connectionConfiguration, credentials),
            DatabaseManagementSystemType.PostgreSql => GetNpgsqlConnectionString(connectionConfiguration, credentials),
            DatabaseManagementSystemType.MySql => GetMySqlConnectionString(connectionConfiguration, credentials),
            _ => string.Empty,
        };
    }

    private static string GetMicrosoftSqlServerConnectionString(ConnectionConfigurationBase connectionConfiguration,
        CredentialsBase credentials)
    {
        var microsoftCredentials = (MicrosoftCredentials)credentials;
        var microsoftConnectionConfiguration = (MicrosoftConnectionConfiguration)connectionConfiguration;
        var connection = new StringBuilder();
        connection.Append(
            $"Server={microsoftConnectionConfiguration.Server};Database={microsoftConnectionConfiguration.DatabaseName};");
        connection.Append($"Integrated Security={microsoftConnectionConfiguration.IntegratedSecurity};");
        connection.Append(
            $"Persist Security Info={microsoftConnectionConfiguration.PersistSecurityInfo};Pooling={microsoftConnectionConfiguration.Pooling};");
        connection.Append(
            $"Encrypt={microsoftConnectionConfiguration.Encrypt};TrustServerCertificate={microsoftConnectionConfiguration.TrustServerCertificate};");
        connection.Append($"User Id={microsoftCredentials.Username};Password={microsoftCredentials.Password};");
        return connection.ToString();
    }

    private static string GetNpgsqlConnectionString(ConnectionConfigurationBase connectionConfiguration,
        CredentialsBase credentials)
    {
        var npgsqlCredentials = (NpgsqlCredentials)credentials;
        var npgsqlConnectionConfiguration = (NpgsqlConnectionConfiguration)connectionConfiguration;
        var connection = new StringBuilder();
        connection.Append($"Server={npgsqlConnectionConfiguration.Host};Port={npgsqlConnectionConfiguration.Port};");
        connection.Append($"Database={npgsqlConnectionConfiguration.DatabaseName};User Id={npgsqlCredentials.Username};Password={npgsqlCredentials.Password}");
        return connection.ToString();
    }

    private static string GetMySqlConnectionString(ConnectionConfigurationBase connectionConfiguration,
        CredentialsBase credentials)
    {
        var mysqlCredentials = (MySqlCredentials)credentials;
        var mysqlConnectionConfiguration = (MySqlConnectionConfiguration)connectionConfiguration;
        var connection = new StringBuilder();
        connection.Append($"Server={mysqlConnectionConfiguration.Server};Port={mysqlConnectionConfiguration.Port};");
        connection.Append($"Database={mysqlConnectionConfiguration.DatabaseName};Uid={mysqlCredentials.Username};Pwd={mysqlCredentials.Password};");
        connection.Append($"SslMode={mysqlConnectionConfiguration.SslMode};");
        return connection.ToString();
    }
}