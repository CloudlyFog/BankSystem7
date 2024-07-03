using BankSystem7.ApplicationAggregate.Interfaces;

namespace BankSystem7.UserAggregate;

public interface IUserRepository : IRepository<User>, IRepositoryAsync<User>
{
}