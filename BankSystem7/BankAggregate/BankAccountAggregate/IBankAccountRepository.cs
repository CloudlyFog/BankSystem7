using BankSystem7.ApplicationAggregate.Interfaces;
using BankSystem7.ApplicationAggregate.Interfaces.Readers;
using BankSystem7.UserAggregate;

namespace BankSystem7.BankAggregate.BankAccountAggregate;
public interface IBankAccountRepository : IRepository<BankAccount>,
    IReaderTracking<BankAccount>, IRepositoryAsync<BankAccount>
{
    /// <summary>
    /// The purpose of this method is to transfer a certain amount of money from one user to another
    /// </summary>
    /// <param name="from">The parameter represents the user who is transferring the money</param>
    /// <param name="to">The parameter represents the user who is receiving the money</param>
    /// <param name="transferAmount">The parameter represents the amount of money being transferred</param>
    /// <returns></returns>
    public ExceptionModel Transfer(User? from, User? to, decimal transferAmount);

    /// <summary>
    /// The purpose of this method is to transfer a certain amount of money from one user to another
    /// </summary>
    /// <param name="from">The parameter represents the user who is transferring the money</param>
    /// <param name="to">The parameter represents the user who is receiving the money</param>
    /// <param name="transferAmount">The parameter represents the amount of money being transferred</param>
    /// <returns></returns>
    public Task<ExceptionModel> TransferAsync(User? from, User? to, decimal transferAmount, CancellationToken cancellationToken = default);
}
