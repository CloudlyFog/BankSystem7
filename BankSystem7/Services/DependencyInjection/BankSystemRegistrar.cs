using BankSystem7.Services.Interfaces.Base;
using Microsoft.Extensions.DependencyInjection;

namespace BankSystem7.Services.DependencyInjection;

public sealed class BankSystemRegistrar
{
    public static ServiceProvider Inject(IDependencyInjectionRegistrar[] dependencies, Type[] dependenciesTypes)
    {
        if (dependencies.Length != dependenciesTypes.Length)
            throw new InvalidOperationException();

        var services = new ServiceCollection();
        for (int i = 0; i < dependencies.Length; i++)
            services.AddSingleton(dependenciesTypes[i], dependencies[i]);

        return services.BuildServiceProvider();
    }
}