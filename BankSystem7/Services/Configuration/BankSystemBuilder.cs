using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using BankSystem7.Services.Repositories;
using System.Reflection;

namespace BankSystem7.Services.Configuration;

public class BankSystemBuilder<TUser, TCard, TBankAccount, TBank, TCredit> : BuilderBase<TUser, TCard, TBankAccount, TBank, TCredit>
where TUser : User
where TCard : Card
where TBankAccount : BankAccount
where TBank : Bank
where TCredit : Credit
{
    public override IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>? Build()
    {
        if (!BuilderSettings.BuildBankAccountRepository)
            return null;

        var thisMethods = GetType().GetRuntimeMethods();
        foreach (var builderSettingsProperties in BuilderSettings.GetType().GetProperties())
        {
            if (thisMethods.Any(method => method.Name.Equals(builderSettingsProperties.Name)))
                thisMethods.FirstOrDefault(method => method.Name.Equals(builderSettingsProperties.Name))?.Invoke(this, null);
        }

        return ServiceConfiguration;
    }

    public override void BuildBankAccountRepository()
    {
        ServiceConfiguration.BankAccountRepository =
            new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
    }

    public override void BuildBankRepository()
    {
        ServiceConfiguration.BankRepository =
            new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
    }

    public override void BuildCardRepository()
    {
        ServiceConfiguration.CardRepository =
            new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServiceConfiguration.BankAccountRepository);
    }

    public override void BuildCreditRepository()
    {
        ServiceConfiguration.CreditRepository =
            new CreditRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
    }

    public override void BuildLogger()
    {
        if (ServiceConfiguration?.Options?.LoggerOptions is null)
            return;

        if (ServiceConfiguration.Options.LoggerOptions.IsEnabled)
            ServiceConfiguration.LoggerRepository = new LoggerRepository(ServiceConfiguration.Options.LoggerOptions);

    }

    public override void BuildLoggerRepository()
    {
        if (ServiceConfiguration?.Options?.LoggerOptions is null)
            return;

        if (ServiceConfiguration.Options.LoggerOptions.IsEnabled)
            ServiceConfiguration.Logger = new Logger<TUser, TCard, TBankAccount, TBank, TCredit>(ServiceConfiguration.LoggerRepository, ServiceConfiguration.Options.LoggerOptions);
    }

    public override void BuildOperationRepository()
    {
        ServiceConfiguration.OperationRepository = new OperationRepository(ServiceConfiguration.Options.OperationOptions);
    }

    public override void BuildUserRepository()
    {
        ServiceConfiguration.UserRepository =
            new UserRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServiceConfiguration.BankAccountRepository);
    }
}