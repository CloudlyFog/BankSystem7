using BankSystem7.ApplicationAggregate.Entities;
using BankSystem7.ApplicationAggregate.Interfaces.Readers;
using BankSystem7.ApplicationAggregate.Interfaces.Writers;

namespace BankSystem7.ApplicationAggregate.Interfaces;

public interface IRepositoryAsync<TEntity> : IBase<TEntity>, IReaderAsync<TEntity>, IWriterAsync<TEntity> where TEntity : Entity
{
}