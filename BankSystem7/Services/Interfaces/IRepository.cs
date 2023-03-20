using System.Linq.Expressions;
using BankSystem7.Models;

namespace BankSystem7.Services.Interfaces
{
    public interface IRepository<T> : IDisposable
    {
        ExceptionModel Create(T item);
        ExceptionModel Update(T item);
        ExceptionModel Delete(T item);
        IEnumerable<T> All { get; }
        T Get(Expression<Func<T, bool>> predicate);
        bool Exist(Expression<Func<T, bool>> predicate);
    }
}
