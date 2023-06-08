using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Repositories;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Threading.Tasks;

namespace BankSystem7.Services.Interfaces;

public abstract class BuilderBase<TUser, TCard, TBankAccount, TBank, TCredit>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private bool _disposed;

    protected BuilderBase()
    {
        ServiceConfiguration = new ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>();
        ServiceConfiguration.Options = new ConfigurationOptions();
        ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfigurationOptions = ServiceConfiguration.Options;
    }

    protected BuilderBase(BuilderSettings settings)
    {
        BuilderSettings = settings;
        ServiceConfiguration = new ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>();
        ServiceConfiguration.Options = new ConfigurationOptions();
        ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfigurationOptions = ServiceConfiguration.Options;
    }

    protected BuilderBase(ConfigurationOptions options)
    {
        ServiceConfiguration.Options = options;
        ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfigurationOptions = options;
    }

    protected BuilderBase(BuilderSettings builderSettings, ConfigurationOptions options)
    {
        BuilderSettings = builderSettings;
        ServiceConfiguration.Options = options;
        ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfigurationOptions = options;
    }

    public BuilderSettings BuilderSettings { get; set; } = new();
    protected IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit> ServiceConfiguration { get; }

    public abstract IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>? Build();
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

        ServiceConfiguration?.Dispose();

        _disposed = true;
    }
}
