namespace BankSystem7.Services.Configuration;

public sealed class BuilderSettings
{
    public bool BuildBankAccountRepository { get; set; }
    public bool BuildUserRepository { get; set; }
    public bool BuildCardRepository { get; set; }
    public bool BuildBankRepository { get; set; }
    public bool BuildCreditRepository { get; set; }
    public bool BuildLoggerRepository { get; set; }
    public bool BuildOperationRepository { get; set; }
    public bool BuildLogger { get; set; }
}
