using BankSystem7.AppContext;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Configuration;

/// <summary>
/// Manages settings for library
/// </summary>
public class ConfigurationOptions
{
    /// <summary>
    /// Manages database deletion during initializing library
    /// </summary>
    public bool EnsureDeleted { get; init; }
    
    /// <summary>
    /// Manages database creation during initializing library 
    /// </summary>
    public bool EnsureCreated { get; init; } = true;
    
    /// <summary>
    /// Takes connection string for database
    /// </summary>
    public string? Connection { get; init; } = null;
    
    /// <summary>
    /// Takes name of database. If You don't want to change connection details, You can just specify it 
    /// </summary>
    public string? DatabaseName { get; init; } = null;

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
}