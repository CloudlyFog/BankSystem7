using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using BankSystem7.Services.Interfaces.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BankSystem7.Services.DependencyInjection;

public static class BankSystemRegistrar
{
    public static ServiceCollection Inject<TUser, TCard, TBankAccount, TBank, TCredit>
        (IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>? serviceConfiguration)
        where TUser : User
        where TCard : Card
        where TBankAccount : BankAccount
        where TBank : Bank
        where TCredit : Credit
    {
        var serviceCollection = new ServiceCollection();

        var interfaces = serviceConfiguration?.GetType().GetProperties().Where(propertyInfo => propertyInfo.PropertyType.IsInterface) 
                         ?? throw new ArgumentNullException(nameof(serviceConfiguration),
            $"You have to pass not null instance of {serviceConfiguration}. Check passed value of {serviceConfiguration}");

        foreach (var item in interfaces)
        {
            var value = item.GetValue(serviceConfiguration);
            if (value is not null)
                serviceCollection.AddSingleton(item.PropertyType, value);
        }
        
        return serviceCollection;
    }

    public static ServiceCollection Inject(Type[] dependenciesTypes, object[] dependencies)
    {
        return ProvideServices(dependenciesTypes, dependencies);
    }

    public static ServiceCollection Inject(Type[] dependenciesTypes, IDependencyInjectionRegistrar[] dependencies)
    {
        return ProvideServices(dependenciesTypes, dependencies);
    }

    public static ServiceProvider InjectItself(IServiceProvider serviceProvider)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(serviceProvider);
        return serviceCollection.BuildServiceProvider();
    }
    
    public static ServiceCollection Join(this ServiceCollection[] serviceCollections)
    {
        var newServiceCollection = new ServiceCollection();
        foreach (var serviceCollection in serviceCollections)
        {
            foreach (var item in serviceCollection)
            {
                if (item.ImplementationInstance is null)
                {
                    newServiceCollection.AddSingleton(item.ServiceType);
                    continue;
                }
                newServiceCollection.AddSingleton(item.ServiceType, item.ImplementationInstance);
            }
        }

        return newServiceCollection;
    }

    public static ServiceCollection Inject(IEnumerable<Type> dependenciesTypes)
    {
        return ProvideServices(dependenciesTypes);
    }
    
    public static ServiceCollection Inject(IEnumerable<object> dependencies)
    {
        return ProvideServices(dependencies);
    }

    private static void CheckLength(Type[] dependenciesTypes, object[] dependencies)
    {
        if (dependencies.Length != dependenciesTypes.Length)
            throw new InvalidOperationException($"Length of {nameof(dependenciesTypes)} and length of {nameof(dependencies)} is different. " +
                $"It can cause exception {nameof(IndexOutOfRangeException)}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dependenciesTypes"></param>
    /// <param name="dependencies"></param>
    /// <returns></returns>
    private static ServiceCollection ProvideServices(Type[] dependenciesTypes, object[] dependencies)
    {
        CheckLength(dependenciesTypes, dependencies);
        var serviceCollection = new ServiceCollection();
        for (var i = 0; i < dependencies.Length; i++)
            serviceCollection.AddSingleton(dependenciesTypes[i], dependencies[i]);
        
        return serviceCollection;
    }
    
    private static ServiceCollection ProvideServices(IEnumerable<Type> dependenciesTypes)
    {
        var serviceCollection = new ServiceCollection();
        foreach (var dependency in dependenciesTypes)
            serviceCollection.AddSingleton(dependency);
        
        return serviceCollection;
    }
    
    private static ServiceCollection ProvideServices(IEnumerable<object> dependencies)
    {
        var serviceCollection = new ServiceCollection();
        foreach (var dependency in dependencies)
            serviceCollection.AddSingleton(dependency);
        
        return serviceCollection;
    }
}