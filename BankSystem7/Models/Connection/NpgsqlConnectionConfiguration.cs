using BankSystem7.Models.Credentials;
using BankSystem7.Services.Configuration;

namespace BankSystem7.Models.Connection;

public class NpgsqlConnectionConfiguration : ConnectionConfigurationBase
{
    public NpgsqlConnectionConfiguration()
    {
        Credentials = new NpgsqlCredentials();
    }
    public string? Host { get; set; } = ServicesSettings.DefaultHost;
    public string? Port { get; set; } = ServicesSettings.DefaultPort;
}