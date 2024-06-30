using BankSystem7.ApplicationAggregate.Entities;
using BankSystem7.ApplicationAggregate.Interfaces.Readers;
using BankSystem7.ApplicationAggregate.Interfaces.Writers;

namespace BankSystem7.ApplicationAggregate.Interfaces;

public interface IRepository<TEntity> : IBase<TEntity>, IReader<TEntity>, IWriter<TEntity>, IDisposable where TEntity : Entity
{
}