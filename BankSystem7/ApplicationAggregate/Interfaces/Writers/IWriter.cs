using BankSystem7.ApplicationAggregate.Entities;
using BankSystem7.UserAggregate;

namespace BankSystem7.ApplicationAggregate.Interfaces.Writers;

public interface IWriter<in TEntity> where TEntity : Entity
{
    /// <summary>
    /// adds entity to database
    /// </summary>
    /// <param name="entity">inherited model type</param>
    /// <returns></returns>
    ExceptionModel Create(TEntity entity);

    /// <summary>
    /// updates entity in database
    /// </summary>
    /// <param name="entity">inherited model type</param>
    /// <returns></returns>
    ExceptionModel Update(TEntity entity);

    /// <summary>
    /// deletes entity in database
    /// </summary>
    /// <param name="entity">inherited model type</param>
    /// <returns></returns>
    ExceptionModel Delete(TEntity entity);
}