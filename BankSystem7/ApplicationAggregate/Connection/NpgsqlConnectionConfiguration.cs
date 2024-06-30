using BankSystem7.Configuration;
using BankSystem7.ApplicationAggregate.Credentials;

namespace BankSystem7.ApplicationAggregate.Connection;

public class NpgsqlConnectionConfiguration : ConnectionConfigurationBase
{
    public NpgsqlConnectionConfiguration()
    {
        Credentials = new NpgsqlCredentials();
    }

    /// <summary>
    /// Takes name of host for establishing connection with database
    /// </summary>
    public string? Host { get; set; } = ServiceSettings.DefaultHost;

    /// <summary>
    /// Takes port for establishing connection with database
    /// </summary>
    public string? Port { get; set; } = ServiceSettings.DefaultPort;
}