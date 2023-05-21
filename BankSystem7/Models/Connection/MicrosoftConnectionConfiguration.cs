using BankSystem7.Models.Credentials;
using BankSystem7.Services.Configuration;

namespace BankSystem7.Models.Connection;

public class MicrosoftConnectionConfiguration : ConnectionConfigurationBase
{
    public MicrosoftConnectionConfiguration()
    {
        Credentials = new MicrosoftCredentials();
    }

    public bool Pooling { get; set; } = false;
    public bool Encrypt { get; set; } = true;
    public string? Server { get; set; } = ServicesSettings.DefaultServer;
    public string? DataSource { get; set; } = ServicesSettings.DefaultDataSource;
    public bool IntegratedSecurity { get; set; } = false;
    public bool PersistSecurityInfo { get; set; } = false;
    public bool TrustServerCertificate { get; set; } = true;
}