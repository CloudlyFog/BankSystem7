using MySql.Data.MySqlClient;

namespace BankSystem7.ApplicationAggregate.Connection;

public class MySqlConnectionConfiguration : ConnectionConfigurationBase
{
    public MySqlConnectionConfiguration()
    {
        DatabaseManagementSystemType = Configuration.DatabaseManagementSystemType.MySql;
    }
    public string? Server { get; set; }
    public string? Port { get; set; }
    public MySqlSslMode SslMode { get; set; } = MySqlSslMode.Prefered;
}