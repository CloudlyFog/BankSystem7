using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Repositories;

namespace BankSystem7.Services.Interfaces;

public abstract class BuilderBase<TUser, TCard, TBankAccount, TBank, TCredit> : IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private bool _disposed;

    public BuilderSettings BuilderSettings { get; set; } = new();

    public BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankAccountRepository { get; }

    public BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>? BankRepository { get; }

    public CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CardRepository { get; }

    public UserRepository<TUser, TCard, TBankAccount, TBank, TCredit>? UserRepository { get; }

    public CreditRepository<TUser, TCard, TBankAccount, TBank, TCredit>? CreditRepository { get; }

    public LoggerRepository? LoggerRepository { get; }

    public OperationRepository? OperationRepository { get; }

    public ILogger? Logger { get; }

    public void Build()
    {
        var thisMethods = GetType().GetMethods();
        foreach (var builderSettingsProperties in BuilderSettings.GetType().GetProperties())
        {
            if (thisMethods.Any(method => method.Name.Equals(builderSettingsProperties.Name)))
            {
                var method = thisMethods.FirstOrDefault(method => method.Name.Equals(builderSettingsProperties.Name));
                thisMethods.FirstOrDefault(method => method.Name.Equals(builderSettingsProperties.Name))?.Invoke(this, null);
            }
        }
        //BuildBankAccountRepository();
        //BuildUserRepository();
        //BuildCardRepository();
        //BuildBankRepository();
        //BuildCreditRepository();
        //BuildLoggerRepository();
        //BuildOperationRepository();
        //BuildLogger(); 
    }
    internal abstract void BuildBankAccountRepository();
    internal abstract void BuildUserRepository();
    internal abstract void BuildCardRepository();
    internal abstract void BuildBankRepository();
    internal abstract void BuildCreditRepository();
    internal abstract void BuildLoggerRepository();
    internal abstract void BuildOperationRepository();
    internal abstract void BuildLogger();


    public void Dispose()
    {
        if (_disposed)
            return;

        BankAccountRepository?.Dispose();
        BankRepository?.Dispose();
        CardRepository?.Dispose();
        UserRepository?.Dispose();
        CreditRepository?.Dispose();
        OperationRepository?.Dispose();

        _disposed = true;
    }

}

public class Builder : BuilderBase<User, Card, BankAccount, Bank, Credit>
{
    internal override void BuildBankAccountRepository()
    {
        throw new NotImplementedException();
    }

    internal override void BuildBankRepository()
    {
        throw new NotImplementedException();
    }

    internal override void BuildCardRepository()
    {
        throw new NotImplementedException();
    }

    internal override void BuildCreditRepository()
    {
        throw new NotImplementedException();
    }

    internal override void BuildLogger()
    {
        throw new NotImplementedException();
    }

    internal override void BuildLoggerRepository()
    {
        throw new NotImplementedException();
    }

    internal override void BuildOperationRepository()
    {
        throw new NotImplementedException();
    }

    internal override void BuildUserRepository()
    {
        throw new NotImplementedException();
    }
}