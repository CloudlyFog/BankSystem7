using BankSystem7.Models;
using BankSystem7.Models.Connection;
using BankSystem7.Services.Interfaces;
using BankSystem7.Services.Repositories;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace BankSystem7.Services.Configuration;

public class BankSystemBuilder<TUser, TCard, TBankAccount, TBank, TCredit> : BuilderBase<TUser, TCard, TBankAccount, TBank, TCredit>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    public BankSystemBuilder()
    {
    }
    public BankSystemBuilder(BuilderSettings settings) : base(settings)
    {
    }

    public BankSystemBuilder(ConfigurationOptions options) : base(options)
    {
    }

    public BankSystemBuilder(BuilderSettings builderSettings, ConfigurationOptions options) : base(builderSettings, options)
    {
    }

    public override IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>? Build()
    {
        if (!BuilderSettings.BuildBankAccountRepository)
            return null;

        BuildServiceSettings();
        BuildDbContexts();

        var thisMethods = GetType().GetRuntimeMethods();
        foreach (var builderSettingsProperty in BuilderSettings.GetType().GetProperties())
        {
            var buildAccess = builderSettingsProperty.GetValue(BuilderSettings)?.Equals(true);
            if (thisMethods.Any(method => method.Name.Equals(builderSettingsProperty.Name))
                && buildAccess is true)
                thisMethods.FirstOrDefault(method => method.Name.Equals(builderSettingsProperty.Name))?.Invoke(this, null);
        }

        BuildBankServiceOptions();

        return ServiceConfiguration;
    }

    internal override void BuildBankAccountRepository()
    {
        ServiceConfiguration.BankAccountRepository =
            new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
    }

    internal override void BuildBankRepository()
    {
        ServiceConfiguration.BankRepository =
            new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
    }

    internal override void BuildBankServiceOptions()
    {
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration = 
            (ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>)ServiceConfiguration;
    }

    internal override void BuildCardRepository()
    {
        ServiceConfiguration.CardRepository =
            new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServiceConfiguration.BankAccountRepository);
    }

    internal override void BuildCreditRepository()
    {
        ServiceConfiguration.CreditRepository =
            new CreditRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
    }

    internal override void BuildDbContexts()
    {
        var contexts = ServiceConfiguration.Options.Contexts;
        if (contexts is not null && contexts.Count > 0 && !ServicesSettings.Ensured)
            InitializeDbContexts(contexts);
    }

    internal override void BuildLogger()
    {
        if (ServiceConfiguration?.Options?.LoggerOptions is null)
            return;

        if (ServiceConfiguration.Options.LoggerOptions.IsEnabled)
            ServiceConfiguration.LoggerRepository = new LoggerRepository(ServiceConfiguration.Options.LoggerOptions);

    }

    internal override void BuildLoggerRepository()
    {
        if (ServiceConfiguration?.Options?.LoggerOptions is null)
            return;

        if (ServiceConfiguration.Options.LoggerOptions.IsEnabled)
            ServiceConfiguration.Logger = new Logger<TUser, TCard, TBankAccount, TBank, TCredit>(ServiceConfiguration.LoggerRepository, ServiceConfiguration.Options.LoggerOptions);
    }

    internal override void BuildOperationRepository()
    {
        ServiceConfiguration.OperationRepository = new OperationRepository(ServiceConfiguration.Options.OperationOptions);
    }

    internal override void BuildServiceSettings()
    {
        var options = ServiceConfiguration?.Options ?? throw new ArgumentNullException(nameof(ServiceConfiguration.Options));
        ServicesSettings.SetConnection(options.ConnectionConfiguration.DatabaseManagementSystemType, options?.ConnectionConfiguration, options?.Credentials);
        ServicesSettings.EnsureDeleted = options.ConnectionConfiguration.EnsureDeleted;
        ServicesSettings.EnsureCreated = options.ConnectionConfiguration.EnsureCreated;
        ServicesSettings.InitializeAccess = true;
        ServicesSettings.DatabaseManagementSystemType = options.ConnectionConfiguration.DatabaseManagementSystemType;
    }

    internal override void BuildUserRepository()
    {
        ServiceConfiguration.UserRepository =
            new UserRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServiceConfiguration.BankAccountRepository);
    }
}