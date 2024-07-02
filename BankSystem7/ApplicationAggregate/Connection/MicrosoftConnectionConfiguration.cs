using BankSystem7.Configuration;
using BankSystem7.ApplicationAggregate.Credentials;

namespace BankSystem7.ApplicationAggregate.Connection;

public class MicrosoftConnectionConfiguration : ConnectionConfigurationBase
{
    public MicrosoftConnectionConfiguration()
    {
        Credentials = new MicrosoftCredentials();
    }

    /// <summary>
    /// Manages connections pooling
    /// </summary>
    public bool Pooling { get; set; } = false;

    /// <summary>
    /// Manages encrypting TCP/IP traffic
    /// </summary>
    public bool Encrypt { get; set; } = true;

    /// <summary>
    /// Takes server name of sql server
    /// </summary>
    public string? Server { get; set; }

    /// <summary>
    /// Manages Windows Authentication behavior
    /// </summary>
    public IntegratedSecurityType IntegratedSecurity { get; set; } = IntegratedSecurityType.False;

    /// <summary>
    /// Specifies whether the data source can persist sensitive authentication information
    /// such as a password
    /// </summary>
    public bool PersistSecurityInfo { get; set; } = false;

    /// <summary>
    /// Manages server certificates
    /// </summary>
    public bool TrustServerCertificate { get; set; } = false;
}

public enum IntegratedSecurityType
{
    SSPI,
    True,
    False
}