using BankSystem7.Models;

namespace BankSystem7.Services.Interfaces.Base;

public interface IDependencyInjectionRegistrar
{
}

public interface IBase<TEntity> : IDependencyInjectionRegistrar
{
    /// <summary>
    /// returns collection of entities
    /// </summary>
    IQueryable<TEntity> All { get; }
}

public interface IUserRepository<TUser> : IBase<TUser>, IRepository<TUser>, IRepositoryAsync<TUser> where TUser : User
{
}

public interface ICardRepository<TCard> : IBase<TCard>, IRepository<TCard>,
    IReaderServiceWithTracking<TCard>, IRepositoryAsync<TCard> where TCard : Card
{
}

public interface IBankAccountRepository<TUser, TBankAccount> : IBase<TBankAccount>, IRepository<TBankAccount>,
    IReaderServiceWithTracking<TBankAccount>, IRepositoryAsync<TBankAccount>
    where TUser : User
    where TBankAccount : BankAccount
{
    /// <summary>
    /// The purpose of this method is to transfer a certain amount of money from one user to another
    /// </summary>
    /// <param name="from">The parameter represents the user who is transferring the money</param>
    /// <param name="to">The parameter represents the user who is receiving the money</param>
    /// <param name="transferAmount">The parameter represents the amount of money being transferred</param>
    /// <returns></returns>
    public ExceptionModel Transfer(TUser? from, TUser? to, decimal transferAmount);

    /// <summary>
    /// The purpose of this method is to transfer a certain amount of money from one user to another
    /// </summary>
    /// <param name="from">The parameter represents the user who is transferring the money</param>
    /// <param name="to">The parameter represents the user who is receiving the money</param>
    /// <param name="transferAmount">The parameter represents the amount of money being transferred</param>
    /// <returns></returns>
    public Task<ExceptionModel> TransferAsync(TUser? from, TUser? to, decimal transferAmount);
}

public interface IBankRepository<TUser, TBank> : IBase<TBank>, IRepository<TBank>,
    IReaderServiceWithTracking<TBank>, IRepositoryAsync<TBank>
    where TBank : Bank
    where TUser : User
{
    /// <summary>
    /// accrual money to user bank account from bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public ExceptionModel BankAccountAccrual(TUser user, Operation operation);

    /// <summary>
    /// accrual money to user bank account from bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public Task<ExceptionModel> BankAccountAccrualAsync(TUser user, Operation operation);

    /// <summary>
    /// withdraw money from user bank account and accrual to bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public ExceptionModel BankAccountWithdraw(TUser user, Operation operation);

    /// <summary>
    /// withdraw money from user bank account and accrual to bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public Task<ExceptionModel> BankAccountWithdrawAsync(TUser user, Operation operation);

    /// <summary>
    /// Calculates the bank account amount.
    /// </summary>
    /// <param name="accountAmountValue">The account amount value.</param>
    /// <returns></returns>
    internal decimal CalculateBankAccountAmount(decimal accountAmountValue);
}

public interface ICreditRepository<TUser, TCredit> : IBase<TCredit>, IRepository<TCredit>,
    IReaderServiceWithTracking<TCredit>, IRepositoryAsync<TCredit>
    where TUser : User
    where TCredit : Credit
{
    /// <summary>
    /// gives to user credit with the definite amount of money
    /// adds to the table field with credit's data of user
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <returns></returns>
    public ExceptionModel TakeCredit(TUser? user, TCredit? credit);

    /// <summary>
    /// gives to user credit with the definite amount of money
    /// adds to the table field with credit's data of user
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <returns></returns>
    public Task<ExceptionModel> TakeCreditAsync(TUser? user, TCredit? credit);

    /// <summary>
    /// implements paying some amount of credit for full its repaying
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <param name="payAmount">amount of money for paying</param>
    /// <returns></returns>
    public ExceptionModel PayCredit(TUser? user, TCredit credit, decimal payAmount);

    /// <summary>
    /// implements paying some amount of credit for full its repaying
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <param name="payAmount">amount of money for paying</param>
    /// <returns></returns>
    public Task<ExceptionModel> PayCreditAsync(TUser? user, TCredit credit, decimal payAmount);
}

public interface IOperationRepository : IBase<Operation>, IRepository<Operation>
{
}