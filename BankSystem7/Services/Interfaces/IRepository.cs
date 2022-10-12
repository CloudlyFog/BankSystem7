using Standart7.Models;
using System.Linq.Expressions;

namespace BankSystem7.Services.Interfaces
{
    public interface IRepository<T> : IDisposable
    {
        ExceptionModel Create(T item);
        ExceptionModel Update(T item);
        ExceptionModel Delete(T item);
        IEnumerable<T> All { get; }
        T Get(Guid id);
        T Get(Expression<Func<T, bool>> predicate);
        bool Exist(Guid id);
        bool Exist(Expression<Func<T, bool>> predicate);
    }
}
