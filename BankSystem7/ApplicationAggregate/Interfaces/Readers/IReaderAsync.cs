using BankSystem7.ApplicationAggregate.Entities;
using BankSystem7.ApplicationAggregate.Options;

namespace BankSystem7.ApplicationAggregate.Interfaces.Readers;

public interface IReaderAsync<TEntity> where TEntity : Entity
{
    /// <summary>
    /// returns entity with specified predicate from database
    /// if you'll pass only a predicate, it will return an entity from the database
    /// and 'll skip check in cache
    /// </summary>
    /// <param name="entityFindOptions"></param>
    /// <returns></returns>
    Task<TEntity> GetAsync(EntityFindOptions<TEntity> entityFindOptions, CancellationToken cancellationToken = default);

    /// <summary>
    /// returns true or false depending exists entity in the database or not
    /// </summary>
    /// <param name="entityFindOptions"></param>
    /// <returns></returns>
    Task<bool> ExistAsync(EntityFindOptions<TEntity> entityFindOptions, CancellationToken cancellationToken = default);

    /// <summary>
    /// checks if the passed entity meets the conditions
    /// </summary>
    /// <param name="entity">inherited model type</param>
    /// <returns></returns>
    Task<bool> FitsConditionsAsync(TEntity? entity, CancellationToken cancellationToken = default);
}