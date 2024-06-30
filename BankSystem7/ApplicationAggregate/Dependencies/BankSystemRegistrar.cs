using BankSystem7.ApplicationAggregate.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BankSystem7.ApplicationAggregate.Dependencies;

public static class BankSystemRegistrar
{

    public static ServiceCollection Inject(Type[] dependenciesTypes, object[] dependencies)
    {
        return ProvideServices(dependenciesTypes, dependencies);
    }

    public static ServiceCollection Inject(Type[] dependenciesTypes, IDependencyInjectionRegistrar[] dependencies)
    {
        return ProvideServices(dependenciesTypes, dependencies);
    }

    public static ServiceCollection Inject(Dependency[] dependencies)
    {
        return ProvideServices(dependencies);
    }

    public static ServiceCollection Inject(TypedDependency[] dependencies)
    {
        return ProvideServices(dependencies);
    }

    public static ServiceProvider InjectItself(IServiceProvider serviceProvider)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(serviceProvider);
        return serviceCollection.BuildServiceProvider();
    }

    public static ServiceProvider InjectItself(IServiceProvider serviceProvider, ServiceLifetime serviceLifetime)
    {
        return new ServiceCollection
        {
            new ServiceDescriptor(serviceProvider.GetType(), x => serviceProvider, serviceLifetime)
        }.BuildServiceProvider();
    }

    public static ServiceCollection Join(this ServiceCollection[] serviceCollections)
    {
        var newServiceCollection = new ServiceCollection();
        foreach (var serviceCollection in serviceCollections)
        {
            foreach (var dependency in serviceCollection)
            {
                if (dependency?.ImplementationInstance is not null)
                {
                    newServiceCollection.Add(new ServiceDescriptor(dependency.ServiceType,
                        x => dependency?.ImplementationInstance, dependency.Lifetime));
                    continue;
                }
                else if (dependency?.ImplementationFactory is not null)
                {
                    newServiceCollection.Add(new ServiceDescriptor(dependency.ServiceType,
                    dependency?.ImplementationFactory, dependency.Lifetime));
                    continue;
                }

                newServiceCollection.Add(new ServiceDescriptor(dependency.ServiceType,
                    dependency.ImplementationType, dependency.Lifetime));
            }
        }

        return newServiceCollection;
    }

    public static ServiceCollection Inject(IEnumerable<Type> dependenciesTypes)
    {
        return ProvideServices(dependenciesTypes);
    }

    /// <summary>
    /// This method takes in a collection of objects called "dependencies" and returns a ServiceCollection.
    /// It is used to inject the dependencies into the ServiceCollection
    /// </summary>
    /// <param name="dependencies"></param>
    /// <returns></returns>
    public static ServiceCollection Inject(IEnumerable<object> dependencies)
    {
        return ProvideServices(dependencies);
    }

    /// <summary>
    /// This method checks if the lengths of the arrays "dependenciesTypes" and "dependencies" are equal.
    /// If they are not equal, it throws an InvalidOperationException with a specific error message indicating
    /// that the difference in lengths can potentially cause an IndexOutOfRangeException.
    /// </summary>
    /// <param name="dependenciesTypes">array of dependency types</param>
    /// <param name="dependencies">array of dependency objects</param>
    /// <exception cref="InvalidOperationException">caused when lengths of the arrays "dependenciesTypes" and "dependencies" aren't equal</exception>
    private static void CheckLength(Type[] dependenciesTypes, object[] dependencies)
    {
        if (dependencies.Length != dependenciesTypes.Length)
            throw new InvalidOperationException($"Length of {nameof(dependenciesTypes)} and length of {nameof(dependencies)} is different. " +
                $"It can cause exception {nameof(IndexOutOfRangeException)}");
    }

    /// <summary>
    /// This method checks if the lengths of the arrays are equal.
    /// </summary>
    /// <param name="objects">arrays whose length values will be checked</param>
    /// <exception cref="InvalidOperationException">caused when lengths of the arrays "dependenciesTypes" and "dependencies" aren't equal</exception>
    private static void CheckLength(params object[][] objects)
    {
        var currentObjectIndex = 0;
        var nextObjectIndex = 1;

        if (objects.Where(item =>
            {
                if (nextObjectIndex >= objects.Length)
                    return false;
                currentObjectIndex++;
                return item.Length != objects[nextObjectIndex++].Length;
            }).Any())
        {
            throw new InvalidOperationException($"Lengths of arrays with index {{{currentObjectIndex}}} and {{{nextObjectIndex}}} is different. " +
                                                $"It can cause exception {nameof(IndexOutOfRangeException)}");
        }
    }

    /// <summary>
    /// This method is a helper method that is used to provide a collection of services to be used in a dependency injection container.
    /// </summary>
    /// <param name="dependenciesTypes">array of dependency types</param>
    /// <param name="dependencies">array of dependency objects</param>
    /// <returns></returns>
    private static ServiceCollection ProvideServices(Type[] dependenciesTypes, object[] dependencies)
    {
        CheckLength(dependenciesTypes, dependencies);
        var serviceCollection = new ServiceCollection();
        for (var i = 0; i < dependencies.Length; i++)
            serviceCollection.AddSingleton(dependenciesTypes[i], dependencies[i]);

        return serviceCollection;
    }

    /// <summary>
    /// This method is a helper method that is used to provide a collection of services to be used in a dependency injection container.
    /// </summary>
    /// <param name="dependenciesTypes">array of dependency types</param>
    /// <returns></returns>
    private static ServiceCollection ProvideServices(IEnumerable<Type> dependenciesTypes)
    {
        var serviceCollection = new ServiceCollection();
        foreach (var dependency in dependenciesTypes)
            serviceCollection.AddSingleton(dependency);

        return serviceCollection;
    }

    /// <summary>
    /// This method is a helper method that is used to provide a collection of services to be used in a dependency injection container.
    /// </summary>
    /// <param name="dependencies">array of dependency objects</param>
    /// <returns></returns>
    private static ServiceCollection ProvideServices(IEnumerable<object> dependencies)
    {
        var serviceCollection = new ServiceCollection();
        foreach (var dependency in dependencies)
            serviceCollection.AddSingleton(dependency);

        return serviceCollection;
    }

    private static ServiceCollection ProvideServices(Dependency[] dependencies)
    {
        var serviceCollection = new ServiceCollection();
        foreach (var dependency in dependencies)
            serviceCollection.Add(new ServiceDescriptor(dependency.DependencyType,
                x => dependency.DependencyImplementation, dependency.ServiceLifetime));

        return serviceCollection;
    }

    private static ServiceCollection ProvideServices(TypedDependency[] dependencies)
    {
        var serviceCollection = new ServiceCollection();
        foreach (var dependency in dependencies)
            serviceCollection.Add(new ServiceDescriptor(dependency.ServiceType,
                dependency.ImplementationType, dependency.ServiceLifetime));

        return serviceCollection;
    }
}