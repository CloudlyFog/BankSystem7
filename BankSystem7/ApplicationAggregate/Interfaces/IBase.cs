namespace BankSystem7.ApplicationAggregate.Interfaces;

public interface IDependencyInjectionRegistrar
{
}

public interface IBase<out TEntity> : IDependencyInjectionRegistrar
{
    /// <summary>
    /// returns collection of entities
    /// </summary>
    IQueryable<TEntity> Entities { get; }
}