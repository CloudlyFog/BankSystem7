using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BankSystem7.Extensions;

public static class ServiceConfigurationExtensions
{
    public static IServiceCollection AddNationBankSystem<TUser, TCard, TBankAccount, TBank, TCredit>(this IServiceCollection services, Action<ConfigurationOptions> options)
        where TUser : User
        where TCard : Card
        where TBankAccount : BankAccount
        where TBank : Bank
        where TCredit : Credit
    {
        var builder = new BankSystemBuilder<TUser, TCard, TBankAccount, TBank, TCredit>();
        var resultOptions = new ConfigurationOptions();
        options?.Invoke(resultOptions);
        return services.AddSingleton<IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>>(builder.Build());
    }

    public static IServiceCollection AddNationBankSystem<TUser, TCard, TBankAccount, TBank, TCredit>(this IServiceCollection services, ConfigurationOptions options)
        where TUser : User
        where TCard : Card
        where TBankAccount : BankAccount
        where TBank : Bank
        where TCredit : Credit
    {
        var builder = new BankSystemBuilder<TUser, TCard, TBankAccount, TBank, TCredit>();
        return services.AddSingleton<IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>>(builder.Build());
    }
}