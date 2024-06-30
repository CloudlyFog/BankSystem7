using Microsoft.Extensions.DependencyInjection;

namespace BankSystem7.ApplicationAggregate.Dependencies;

public record Dependency(Type DependencyType, object DependencyImplementation, ServiceLifetime ServiceLifetime);

public record TypedDependency(Type ServiceType, Type ImplementationType, ServiceLifetime ServiceLifetime);