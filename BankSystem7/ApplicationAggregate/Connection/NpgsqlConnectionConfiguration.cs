using BankSystem7.ApplicationAggregate.Credentials;
using BankSystem7.ApplicationAggregate.Helpers;

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
    public string? Host { get; set; }

    /// <summary>
    /// Takes port for establishing connection with database
    /// </summary>
    public string? Port { get; set; }
}