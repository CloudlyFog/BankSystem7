using System.Linq.Expressions;
using BankSystem7.Models;

namespace BankSystem7.Services.Interfaces;

public interface IRepository<T> : IDisposable
{
    /// <summary>
    /// adds entity of <see cref="item"/> to database
    /// </summary>
    /// <param name="item">inherited model type</param>
    /// <returns></returns>
    ExceptionModel Create(T item);
        
    /// <summary>
    /// updates entity of <see cref="item"/> in database
    /// </summary>
    /// <param name="item">inherited model type</param>
    /// <returns></returns>
    ExceptionModel Update(T item);
        
    /// <summary>
    /// deletes entity of <see cref="item"/> in database
    /// </summary>
    /// <param name="item">inherited model type</param>
    /// <returns></returns>
    ExceptionModel Delete(T item);
        
    /// <summary>
    /// returns collection of entities
    /// </summary>
    IEnumerable<T> All { get; }
        
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