using BankSystem7.ApplicationAggregate.Entities;
using BankSystem7.UserAggregate;

namespace BankSystem7.ApplicationAggregate.Interfaces.Writers;

public interface IWriterAsync<in TEntity> where TEntity : Entity
{
    /// <summary>
    /// adds entity to database
    /// </summary>
    /// <param name="entity">inherited model type</param>
    /// <returns></returns>
    Task<ExceptionModel> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// updates entity in database
    /// </summary>
    /// <param name="entity">inherited model type</param>
    /// <returns></returns>
    Task<ExceptionModel> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// deletes entity in database
    /// </summary>
    /// <param name="entity">inherited model type</param>
    /// <returns></returns>
    Task<ExceptionModel> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}