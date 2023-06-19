using BankSystem7.Models;

namespace BankSystem7.Services.Interfaces.Base;

public interface IUserRepository<TUser> : IRepository<TUser>, IRepositoryAsync<TUser> where TUser : User
{
    /// <summary>
    /// returns collection of entities
    /// </summary>
    IQueryable<TUser> All { get; }
}

public interface ICardRepository<TCard> : IRepository<TCard>,
    IReaderServiceWithTracking<TCard>, IRepositoryAsync<TCard> where TCard : Card
{
    /// <summary>
    /// returns collection of entities
    /// </summary>
    IQueryable<TCard> All { get; }
}

public interface IBankAccountRepository<TBankAccount> : IRepository<TBankAccount>,
    IReaderServiceWithTracking<TBankAccount>, IRepositoryAsync<TBankAccount> where TBankAccount : BankAccount
{
    /// <summary>
    /// returns collection of entities
    /// </summary>
    IQueryable<TBankAccount> All { get; }
}

public interface IBankRepository<TBank> : IRepository<TBank>,
    IReaderServiceWithTracking<TBank>, IRepositoryAsync<TBank> where TBank : Bank
{
    /// <summary>
    /// returns collection of entities
    /// </summary>
    IQueryable<TBank> All { get; }
}

public interface ICreditRepository<TCredit> : IRepository<TCredit>,
    IReaderServiceWithTracking<TCredit>, IRepositoryAsync<TCredit> where TCredit : Credit
{
    /// <summary>
    /// returns collection of entities
    /// </summary>
    IQueryable<TCredit> All { get; }
}

public interface IOperationRepository : IRepository<Operation>
{
    /// <summary>
    /// returns collection of entities
    /// </summary>
    IQueryable<Operation> All { get; }
}