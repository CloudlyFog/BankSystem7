using BankSystem7.Services.Configuration;
using MySql.Data.MySqlClient;

namespace BankSystem7.Models.Connection;

public class MySqlConnectionConfiguration : ConnectionConfigurationBase
{
    public string? Server { get; set; } = ServicesSettings.DefaultServer;
    public string? Port { get; set; } = ServicesSettings.DefaultPort;
    public MySqlSslMode SslMode { get; set; } = MySqlSslMode.Prefered;
}