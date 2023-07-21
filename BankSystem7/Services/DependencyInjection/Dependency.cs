using Microsoft.Extensions.DependencyInjection;

namespace BankSystem7.Services.DependencyInjection;

public record Dependency(Type DependencyType, object DependencyImplementation, ServiceLifetime ServiceLifetime);


public record TypedDependency(Type ServiceType, Type ImplementationType, ServiceLifetime ServiceLifetime);
