namespace BankSystem7.Services.Configuration;

public sealed class BuilderSettings
{
    public bool BuildBankAccountRepository { get; set; } = true;
    public bool BuildUserRepository { get; set; } = true;
    public bool BuildCardRepository { get; set; } = true;
    public bool BuildBankRepository { get; set; } = true;
    public bool BuildCreditRepository { get; set; } = true;
    public bool BuildLoggerRepository { get; set; } = true;
    public bool BuildOperationRepository { get; set; } = true;
    public bool BuildLogger { get; set; } = true;
}
