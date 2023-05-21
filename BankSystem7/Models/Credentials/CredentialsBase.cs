namespace BankSystem7.Models.Credentials;

public abstract class CredentialsBase
{
    /// <summary>
    /// Takes user name/id for establishing connection with database
    /// </summary>
    public string? Username { get; set; }
    
    /// <summary>
    /// Takes password of user for establishing connection with database
    /// </summary>
    public string? Password { get; set; }
}