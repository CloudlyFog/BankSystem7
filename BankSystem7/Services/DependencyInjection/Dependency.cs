using Microsoft.Extensions.DependencyInjection;

namespace BankSystem7.Services.DependencyInjection;

public class Dependency
{
    public required Type DependencyType { get; init; }
    public required object DependencyImplementation { get; init; } 
    public required ServiceLifetime ServiceLifetime { get; init; }
}

public class DependencyCollection : ServiceCollection
{
}