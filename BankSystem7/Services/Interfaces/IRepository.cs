using BankSystem7.Models;
using System.Linq.Expressions;

namespace BankSystem7.Services.Interfaces;

public interface IRepository<T> : IReaderService<T>, IWriterService<T>, IDisposable where T : class
{
    /// <summary>
    /// checks if the passed item meets the conditions
    /// </summary>
    /// <param name="item">inherited model type</param>
    /// <returns></returns>
    bool FitsConditions(T? item);
}

public interface IReaderService<T> where T : class
{
    /// <summary>
    /// returns collection of entities
    /// </summary>
    IQueryable<T> All { get; }

    /// <summary>
    /// returns entity with specified predicate from database
    /// </summary>
    /// <param name="predicate">predicate for function expression</param>
    /// <returns></returns>
    T Get(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// returns true or false depending exists entity in the database or not
    /// </summary>
    /// <param name="predicate">predicate for function expression</param>
    /// <returns></returns>
    bool Exist(Expression<Func<T, bool>> predicate);
}

public interface IReaderServiceWithTracking<T> where T : class
{
    /// <summary>
    /// returns collection of entities
    /// </summary>
    IQueryable<T> AllWithTracking { get; }

    /// <summary>
    /// returns entity with specified predicate from database
    /// </summary>
    /// <param name="predicate">predicate for function expression</param>
    /// <returns></returns>
    T GetWithTracking(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// returns true or false depending exists entity in the database or not
    /// </summary>
    /// <param name="predicate">predicate for function expression</param>
    /// <returns></returns>
    bool ExistWithTracking(Expression<Func<T, bool>> predicate);
}

public interface IWriterService<in T> where T : class
{
    /// <summary>
    /// adds entity to database
    /// </summary>
    /// <param name="item">inherited model type</param>
    /// <returns></returns>
    ExceptionModel Create(T item);

    /// <summary>
    /// updates entity in database
    /// </summary>
    /// <param name="item">inherited model type</param>
    /// <returns></returns>
    ExceptionModel Update(T item);

    /// <summary>
    /// deletes entity in database
    /// </summary>
    /// <param name="item">inherited model type</param>
    /// <returns></returns>
    ExceptionModel Delete(T item);
}