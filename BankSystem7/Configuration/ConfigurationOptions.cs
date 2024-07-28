using System.Reflection;
using BankSystem7.ApplicationAggregate.Connection;
using BankSystem7.ApplicationAggregate.Credentials;
using BankSystem7.BankAggregate.OperationAggregate;

namespace BankSystem7.Configuration;

/// <summary>
/// Manages settings for library
/// </summary>
public class ConfigurationOptions
{
    public ConnectionSinkConfiguration Database { get; }
}

public class ConnectionSinkConfiguration
{
    private bool _isDatabaseProviderSet;
    private bool _isModelConfigurationSet;
    private bool _isOperationServiceSet;
    
    /// <summary>
    /// From where will be token implementations of <see cref="ModelConfiguration"/>.
    /// Only this implementation will be used
    /// </summary>
    internal Assembly ModelConfigurationAssembly { get; set; }

    /// <summary>
    /// Takes settings for OperationService (i.e. OperationRepository)
    /// <seealso cref="OperationService{T}"/>
    /// </summary>
    internal OperationServiceOptions OperationOptions { get; set; }
    internal CredentialsBase Credentials { get; private set; }
    internal ConnectionConfigurationBase ConnectionConfiguration { get; private set; }

    public ConnectionSinkConfiguration UseSqlServer(MicrosoftConnectionConfiguration? connectionConfiguration, MicrosoftCredentials? credentials)
    {
        ArgumentNullException.ThrowIfNull(connectionConfiguration);
        if (connectionConfiguration.IntegratedSecurity == IntegratedSecurityType.False)
            return UseProvider(connectionConfiguration, credentials);

        return UseProvider(connectionConfiguration, credentials, skipCredentialsCheck: true);
    }

    public ConnectionSinkConfiguration UseSqlServer(Action<MicrosoftConnectionConfiguration?> connectionConfiguration, Action<MicrosoftCredentials?> credentials)
    {
        var connectionConfigurationOptions = new MicrosoftConnectionConfiguration();
        var credentialsOptions = new MicrosoftCredentials();

        connectionConfiguration.Invoke(connectionConfigurationOptions);
        credentials?.Invoke(credentialsOptions);

        return UseSqlServer(connectionConfigurationOptions, credentialsOptions);
    }

    public ConnectionSinkConfiguration UseSqlServer(MicrosoftConnectionConfiguration? connectionConfiguration, IntegratedSecurityType integratedSecurity = IntegratedSecurityType.True)
    {
        ArgumentNullException.ThrowIfNull(connectionConfiguration);
        if (integratedSecurity == IntegratedSecurityType.False)
            throw new ArgumentException("Value of passed argument integratedSecurity can't be {FALSE} in such configuration");
        connectionConfiguration.IntegratedSecurity = integratedSecurity;
        return UseSqlServer(connectionConfiguration, null);
    }

    public ConnectionSinkConfiguration UseSqlServer(Action<MicrosoftConnectionConfiguration?> connectionConfiguration, IntegratedSecurityType integratedSecurity = IntegratedSecurityType.True)
    {
        var connectionConfigurationOptions = new MicrosoftConnectionConfiguration();
        connectionConfiguration.Invoke(connectionConfigurationOptions);

        return UseSqlServer(connectionConfigurationOptions, integratedSecurity);
    }

    public ConnectionSinkConfiguration UseNpgsql(NpgsqlConnectionConfiguration? connectionConfiguration, NpgsqlCredentials? credentials)
    {
        return UseProvider(connectionConfiguration, credentials);
    }

    public ConnectionSinkConfiguration UseNpgsql(Action<NpgsqlConnectionConfiguration?> connectionConfiguration, Action<NpgsqlCredentials?> credentials)
    {
        var connectionConfigurationOptions = new NpgsqlConnectionConfiguration();
        var credentialsOptions = new NpgsqlCredentials();

        connectionConfiguration.Invoke(connectionConfigurationOptions);
        credentials.Invoke(credentialsOptions);

        return UseNpgsql(connectionConfigurationOptions, credentialsOptions);
    }

    public ConnectionSinkConfiguration UseMySql(MySqlConnectionConfiguration? connectionConfiguration, MySqlCredentials? credentials)
    {
        return UseProvider(connectionConfiguration, credentials);
    }

    public ConnectionSinkConfiguration UseMySql(Action<MySqlConnectionConfiguration?> connectionConfiguration, Action<MySqlCredentials?> credentials)
    {
        var connectionConfigurationOptions = new MySqlConnectionConfiguration();
        var credentialsOptions = new MySqlCredentials();

        connectionConfiguration.Invoke(connectionConfigurationOptions);
        credentials.Invoke(credentialsOptions);

        return UseMySql(connectionConfigurationOptions, credentialsOptions);
    }

    public ConnectionSinkConfiguration UseModelConfiguration(Assembly? modelConfigurationAssembly)
    {
        if (_isModelConfigurationSet)
            return this;

        ArgumentNullException.ThrowIfNull(modelConfigurationAssembly);

        ModelConfigurationAssembly = modelConfigurationAssembly;
        _isModelConfigurationSet = true;
        return this;
    }

    public ConnectionSinkConfiguration UseOperationService(OperationServiceOptions? operationServiceOptions)
    {
        if (_isOperationServiceSet)
            return this;

        ArgumentNullException.ThrowIfNull(operationServiceOptions);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationServiceOptions.DatabaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationServiceOptions.CollectionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationServiceOptions.Connection);

        OperationOptions = operationServiceOptions;
        _isOperationServiceSet = true;
        return this;
    }

    private ConnectionSinkConfiguration UseProvider(ConnectionConfigurationBase? connectionConfiguration, CredentialsBase? credentials, bool skipCredentialsCheck = false)
    {
        if (_isDatabaseProviderSet)
            return this;

        ArgumentNullException.ThrowIfNull(connectionConfiguration);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionConfiguration.DatabaseName);

        if (!skipCredentialsCheck)
        {
            ArgumentNullException.ThrowIfNull(credentials);
            ArgumentException.ThrowIfNullOrWhiteSpace(credentials.Username);
            ArgumentException.ThrowIfNullOrWhiteSpace(credentials.Password);
        }

        ConnectionConfiguration = connectionConfiguration;
        Credentials = credentials;
        _isDatabaseProviderSet = true;
        return this;
    }
}

public enum DatabaseManagementSystemType
{
    MicrosoftSqlServer,
    PostgreSql,
    MySql
}