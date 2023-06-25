using BankSystem7.Models;
using BankSystem7.Services.Interfaces.Base;
using Microsoft.Extensions.DependencyInjection;

namespace BankSystem7.Services.DependencyInjection;

public sealed class BankSystemRegistrar
{
    public static ServiceProvider Inject<TUser, TCard, TBankAccount, TBank, TCredit>()
        where TUser : User
        where TCard : Card
        where TBankAccount : BankAccount
        where TBank : Bank
        where TCredit : Credit
    {
        var services = new ServiceCollection();

        var serviceConfiguration = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration;
        var interfaces = serviceConfiguration.GetType().GetProperties();

        foreach (var item in interfaces)
            services.AddSingleton(item.PropertyType, item.GetValue(serviceConfiguration));

        return services.BuildServiceProvider();
    }

    public static ServiceProvider Inject(Type[] dependenciesTypes, object[] dependencies)
    {
        CheckLength(dependenciesTypes, dependencies);

        var services = new ServiceCollection();
        for (int i = 0; i < dependencies.Length; i++)
            services.AddSingleton(dependenciesTypes[i], dependencies[i]);

        return services.BuildServiceProvider();
    }

    public static ServiceProvider Inject(Type[] dependenciesTypes, IDependencyInjectionRegistrar[] dependencies)
    {
        CheckLength(dependenciesTypes, dependencies);

        var services = new ServiceCollection();
        for (int i = 0; i < dependencies.Length; i++)
            services.AddSingleton(dependenciesTypes[i], dependencies[i]);

        return services.BuildServiceProvider();
    }

    private static void CheckLength(Type[] dependenciesTypes, object[] dependencies)
    {
        if (dependencies.Length != dependenciesTypes.Length)
            throw new InvalidOperationException($"Length of {nameof(dependenciesTypes)} and {nameof(dependencies)} is different. " +
                $"It can cause exception {nameof(IndexOutOfRangeException)}");
    }
}