using BankSystem7.Models.Credentials;
using BankSystem7.Services.Configuration;

namespace BankSystem7.Models.Connection;

public class NpgsqlConnectionConfiguration : ConnectionConfigurationBase
{
    public NpgsqlConnectionConfiguration()
    {
        Credentials = new NpgsqlCredentials();
    }

    /// <summary>
    /// Takes name of host for establishing connection with database
    /// </summary>
    public string? Host { get; set; } = ServicesSettings.DefaultHost;

    /// <summary>
    /// Takes port for establishing connection with database
    /// </summary>
    public string? Port { get; set; } = ServicesSettings.DefaultPort;
}