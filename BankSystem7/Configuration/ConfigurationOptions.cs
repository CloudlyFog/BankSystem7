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
    public Assembly ModelConfigurationAssembly { get; set; }

    /// <summary>
    /// Takes settings for OperationService (i.e. OperationRepository)
    /// <seealso cref="OperationService{T}"/>
    /// </summary>
    public OperationServiceOptions? OperationOptions { get; set; }

    public CredentialsBase Credentials { get; set; }
    public ConnectionConfigurationBase ConnectionConfiguration { get; set; }
}

public enum DatabaseManagementSystemType
{
    MicrosoftSqlServer,
    PostgreSql,
    MySql
}