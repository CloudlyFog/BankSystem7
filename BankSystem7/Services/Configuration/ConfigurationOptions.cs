namespace BankSystem7.Services.Configuration;

public sealed class ConfigurationOptions
{
    public bool EnsureDeleted { get; set; }
    public bool EnsureCreated { get; set; } = true;
    public string? Connection { get; set; } = null;
    public string? DatabaseName { get; set; } = null;

    public LoggerOptions? LoggerOptions { get; set; } = new()
    {
        IsEnabled = false,
    };

    public OperationServiceOptions? OperationOptions { get; set; }
}