using BankSystem7.ApplicationAggregate.Interfaces;
using BankSystem7.ApplicationAggregate.Interfaces.Readers;
using BankSystem7.BankAggregate.OperationAggregate;
using BankSystem7.UserAggregate;

namespace BankSystem7.BankAggregate;
public interface IBankRepository : IRepository<Bank>,
    IReaderTracking<Bank>, IRepositoryAsync<Bank>
{
    /// <summary>
    /// accrual money to user bank account from bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public ExceptionModel BankAccountAccrual(User user, Operation operation);

    /// <summary>
    /// accrual money to user bank account from bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public Task<ExceptionModel> BankAccountAccrualAsync(User user, Operation operation, CancellationToken cancellation = default);

    /// <summary>
    /// withdraw money from user bank account and accrual to bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public ExceptionModel BankAccountWithdraw(User user, Operation operation);

    /// <summary>
    /// withdraw money from user bank account and accrual to bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public Task<ExceptionModel> BankAccountWithdrawAsync(User user, Operation operation, CancellationToken cancellation = default);

    /// <summary>
    /// Calculates the bank account amount.
    /// </summary>
    /// <param name="accountAmountValue">The account amount value.</param>
    /// <returns></returns>
    public decimal CalculateBankAccountAmount(decimal accountAmountValue);
}
