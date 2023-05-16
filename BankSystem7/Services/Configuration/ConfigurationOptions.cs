using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Configuration;

public class ConfigurationOptions
{
    public bool EnsureDeleted { get; set; }
    public bool EnsureCreated { get; init; } = true;
    public string? Connection { get; init; } = null;
    public string? DatabaseName { get; init; } = null;

    public LoggerOptions? LoggerOptions { get; init; } = new()
    {
        IsEnabled = false,
    };

    public OperationServiceOptions? OperationOptions { get; init; }

    public Dictionary<DbContext, ModelConfiguration?>? Contexts { get; init; }
}