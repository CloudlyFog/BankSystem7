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
        // Check if BuildBankAccountRepository is set to true in BuilderSettings, if not return null
        if (!BuilderSettings.BuildBankAccountRepository)
            return null;

        // Call BuildServiceSettings method
        BuildServiceSettings();

        // Call BuildDbContexts method
        BuildDbContexts();

        // Get all methods in the current class
        var thisMethods = GetType().GetRuntimeMethods();

        // Loop through all properties in BuilderSettings
        foreach (var builderSettingsProperty in BuilderSettings.GetType().GetProperties())
        {
            // Check if the property value is true
            var buildAccess = builderSettingsProperty.GetValue(BuilderSettings)?.Equals(true);

            // Check if the current class has a method with the same name as the property
            if (thisMethods.Any(method => method.Name.Equals(builderSettingsProperty.Name))
                && buildAccess is true)
            {
                // Call the method with the same name as the property
                thisMethods.FirstOrDefault(method => method.Name.Equals(builderSettingsProperty.Name))?.Invoke(this, null);
            }
        }

        // Call BuildBankServiceOptions method
        BuildBankServiceOptions();

        // Return ServiceConfiguration
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