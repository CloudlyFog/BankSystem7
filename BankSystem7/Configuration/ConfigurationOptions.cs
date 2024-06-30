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
    /// <summary>
    /// From where will be token implementations of <see cref="ModelConfiguration"/>.
    /// Only this implementation will be used
    /// </summary>
    public Assembly ModelConfigurationAssembly { get; init; }

    /// <summary>
    /// Takes settings for OperationService (i.e. OperationRepository)
    /// <seealso cref="OperationService{T}"/>
    /// </summary>
    public OperationServiceOptions? OperationOptions { get; init; }

    public CredentialsBase Credentials { get; init; }
    public ConnectionConfigurationBase ConnectionConfiguration { get; init; }
}

public enum DatabaseManagementSystemType
{
    MicrosoftSqlServer,
    PostgreSql,
    MySql
}