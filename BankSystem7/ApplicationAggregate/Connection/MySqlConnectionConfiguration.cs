using BankSystem7.Configuration;
using MySql.Data.MySqlClient;

namespace BankSystem7.ApplicationAggregate.Connection;

public class MySqlConnectionConfiguration : ConnectionConfigurationBase
{
    public string? Server { get; set; } = ServiceSettings.DefaultServer;
    public string? Port { get; set; } = ServiceSettings.DefaultPort;
    public MySqlSslMode SslMode { get; set; } = MySqlSslMode.Prefered;
}