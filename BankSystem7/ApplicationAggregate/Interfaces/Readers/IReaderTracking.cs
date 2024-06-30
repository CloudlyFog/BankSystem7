using BankSystem7.ApplicationAggregate.Entities;
using BankSystem7.ApplicationAggregate.Options;

namespace BankSystem7.ApplicationAggregate.Interfaces.Readers;

public interface IReaderTracking<TEntity> where TEntity : Entity
{
    /// <summary>
    /// returns collection of entities
    /// </summary>
    IQueryable<TEntity> EntitiesTracking { get; }

    /// <summary>
    /// returns entity with specified predicate from database
    /// </summary>
    /// <param name="entityFindOptions"></param>
    /// <returns></returns>
    TEntity GetTracking(EntityFindOptions<TEntity> entityFindOptions);

    /// <summary>
    /// returns true or false depending exists entity in the database or not
    /// </summary>
    /// <param name="entityFindOptions"></param>
    /// <returns></returns>
    bool ExistTracking(EntityFindOptions<TEntity> entityFindOptions);
}