using BankSystem7.ApplicationAggregate.Interfaces;
using BankSystem7.ApplicationAggregate.Interfaces.Readers;
using BankSystem7.UserAggregate;

namespace BankSystem7.BankAggregate.CreditAggregate;

public interface ICreditRepository : IRepository<Credit>,
    IReaderTracking<Credit>, IRepositoryAsync<Credit>
{
    /// <summary>
    /// gives to user credit with the definite amount of money
    /// adds to the table field with credit's data of user
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <returns></returns>
    public ExceptionModel TakeCredit(User? user, Credit? credit);

    /// <summary>
    /// gives to user credit with the definite amount of money
    /// adds to the table field with credit's data of user
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <returns></returns>
    public Task<ExceptionModel> TakeCreditAsync(User? user, Credit? credit, CancellationToken cancellationToken = default);

    /// <summary>
    /// implements paying some amount of credit for full its repaying
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <param name="payAmount">amount of money for paying</param>
    /// <returns></returns>
    public ExceptionModel PayCredit(User? user, Credit credit, decimal payAmount);

    /// <summary>
    /// implements paying some amount of credit for full its repaying
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <param name="payAmount">amount of money for paying</param>
    /// <returns></returns>
    public Task<ExceptionModel> PayCreditAsync(User? user, Credit credit, decimal payAmount, CancellationToken cancellationToken = default);
}