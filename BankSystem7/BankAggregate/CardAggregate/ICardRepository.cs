using BankSystem7.ApplicationAggregate.Interfaces;
using BankSystem7.ApplicationAggregate.Interfaces.Readers;

namespace BankSystem7.BankAggregate.CardAggregate;
public interface ICardRepository : IRepository<Card>,
    IReaderTracking<Card>, IRepositoryAsync<Card>
{
}