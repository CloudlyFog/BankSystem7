using BankSystem7.AppContext;
using BankSystem7.Models.Connection;
using BankSystem7.Models.Credentials;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Configuration;

/// <summary>
/// Manages settings for library
/// </summary>
public class ConfigurationOptions
{
    /// <summary>
    /// Takes settings for logger
    /// <seealso cref="Logger{TUser,TCard,TBankAccount,TBank,TCredit}"/>
    /// </summary>
    public LoggerOptions? LoggerOptions { get; init; } = new()
    {
        IsEnabled = false,
    };

    /// <summary>
    /// Takes settings for OperationService (i.e. OperationRepository)
    /// <seealso cref="OperationService{T}"/>
    /// </summary>
    public OperationServiceOptions? OperationOptions { get; init; }

    /// <summary>
    /// Manages models configuration.
    /// Key - db context class that inherit from <see cref="GenericDbContext"/> and contains DbSet for models.
    /// Value - <see cref="ModelConfiguration"/> class that tunes relationships between models.
    /// <seealso cref="DbContext"/> <seealso cref="ModelBuilder"/>
    /// </summary>
    public Dictionary<DbContext, ModelConfiguration?>? Contexts { get; init; }
    
    public CredentialsBase Credentials { get; set; }
    public ConnectionConfigurationBase ConnectionConfiguration { get; set; }
}

public enum DatabaseManagementSystemType
{
    MicrosoftSqlServer,
    PostgreSql,
    MySql
}