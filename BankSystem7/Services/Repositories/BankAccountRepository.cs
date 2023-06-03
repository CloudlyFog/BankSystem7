using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;

namespace BankSystem7.Services.Repositories;

public sealed class BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit> : IRepository<TBankAccount>, 
    IReaderServiceWithTracking<TBankAccount>, IRepositoryAsync<TBankAccount>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private readonly BankRepository<TUser, TCard, TBankAccount, TBank, TCredit> _bankRepository;
    private readonly BankContext<TUser, TCard, TBankAccount, TBank, TCredit> _bankContext;
    private readonly ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> _applicationContext;
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="BankAccountRepository{TUser, TCard, TBankAccount, TBank, TCredit}"/> class.
    /// </summary>
    public BankAccountRepository()
    {
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
        _bankContext = _bankRepository.BankContext;
        SetBankServicesOptions();
        _bankRepository = new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BankAccountRepository{TUser, TCard, TBankAccount, TBank, TCredit}"/> class.
    /// </summary>
    /// <param name="bankRepository">The bank repository.</param>
    public BankAccountRepository(BankRepository<TUser, TCard, TBankAccount, TBank, TCredit> bankRepository)
    {
        _bankRepository = bankRepository;
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
        _bankContext = _bankRepository.BankContext;
        SetBankServicesOptions();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BankAccountRepository{TUser, TCard, TBankAccount, TBank, TCredit}"/> class.
    /// </summary>
    /// <param name="connection">The connection string/</param>
    public BankAccountRepository(string connection)
    {
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        _bankContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.BankContext ?? new BankContext<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        SetBankServicesOptions();
        _bankRepository = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration?.BankRepository ?? new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
    }

    public IQueryable<TBankAccount> All =>
        _applicationContext.BankAccounts
            .Include(x => x.Bank)
            .Include(x => x.User.Card)
            .AsNoTracking();

    public IQueryable<TBankAccount> AllWithTracking =>
        _applicationContext.BankAccounts
            .Include(x => x.Bank)
            .Include(x => x.User.Card);

    /// <summary>
    /// The purpose of this method is to transfer a certain amount of money from one user to another
    /// </summary>
    /// <param name="from">The parameter represents the user who is transferring the money</param>
    /// <param name="to">The parameter represents the user who is receiving the money</param>
    /// <param name="transferAmount">The parameter represents the amount of money being transferred</param>
    /// <returns></returns>
    public ExceptionModel Transfer(TUser? from, TUser? to, decimal transferAmount)
    {
        // Check if the sender's bank account, receiver's bank account, and transfer amount are valid. If not, return an operation failed exception.
        if (from?.Card?.BankAccount is null || to?.Card?.BankAccount is null || transferAmount <= 0)
            return ExceptionModel.OperationFailed;

        // Check if the sender's bank account and receiver's bank account exist in the database. If not, return an operation failed exception.
        if (!Exist(x => x.ID == from.Card.BankAccount.ID) || !Exist(x => x.ID == to.Card.BankAccount.ID))
            return ExceptionModel.OperationFailed;

        // Begin a new transaction with repeatable read isolation level.
        using var transaction = _bankContext.Database.BeginTransaction(IsolationLevel.RepeatableRead);

        // Check bank where transaction was sent
        _bankRepository.AnotherBankTransactionOperation = AnotherBankTransactionOperation(from, to);

        // Call the WithdrawAsync method with the sender and transfer amount as parameters and store the result in the withdraw variable.
        var withdraw = Withdraw(from, transferAmount);

        // If the withdraw operation failed, rollback the transaction and return the withdraw exception
        if (withdraw != ExceptionModel.Ok)
        {
            transaction.Rollback();
            return withdraw;
        }

        // Call the AccrualAsync method with the receiver and transfer amount as parameters and store the result in the accrual variable.
        var accrual = Accrual(to, transferAmount);

        // If the accrual operation failed, rollback the transaction and return the withdraw exception
        if (accrual != ExceptionModel.Ok)
        {
            transaction.Rollback();
            return accrual;
        }

        // Commit the transaction.
        transaction.Commit();
        // Return an operation successful exception.
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// The purpose of this method is to transfer a certain amount of money from one user to another
    /// </summary>
    /// <param name="from">The parameter represents the user who is transferring the money</param>
    /// <param name="to">The parameter represents the user who is receiving the money</param>
    /// <param name="transferAmount">The parameter represents the amount of money being transferred</param>
    /// <returns></returns>
    public async Task<ExceptionModel> TransferAsync(TUser? from, TUser? to, decimal transferAmount)
    {
        // Check if the sender's bank account, receiver's bank account, and transfer amount are valid. If not, return an operation failed exception.
        if (from?.Card?.BankAccount is null || to?.Card?.BankAccount is null || transferAmount <= 0)
            return ExceptionModel.OperationFailed;

        // Check if the sender's bank account and receiver's bank account exist in the database. If not, return an operation failed exception.
        if (!Exist(x => x.ID == from.Card.BankAccount.ID) || !Exist(x => x.ID == to.Card.BankAccount.ID))
            return ExceptionModel.OperationFailed;

        // Begin a new transaction with repeatable read isolation level.
        using var transaction = await _bankContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);

        // Check bank where transaction was sent
        _bankRepository.AnotherBankTransactionOperation = AnotherBankTransactionOperation(from, to);

        // Call the WithdrawAsync method with the sender and transfer amount as parameters and store the result in the withdraw variable.
        var withdraw = await WithdrawAsync(from, transferAmount);

        // If the withdraw operation failed, rollback the transaction and return the withdraw exception
        if (withdraw != ExceptionModel.Ok)
        {
            await transaction.RollbackAsync();
            return withdraw;
        }

        // Call the AccrualAsync method with the receiver and transfer amount as parameters and store the result in the accrual variable.
        var accrual = await AccrualAsync(to, transferAmount);

        // If the accrual operation failed, rollback the transaction and return the withdraw exception
        if (accrual != ExceptionModel.Ok)
        {
            await transaction.RollbackAsync();
            return accrual;
        }

        // Commit the transaction.
        await transaction.CommitAsync();
        // Return an operation successful exception.
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// asynchronously accrual money on account with the same user id
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amountAccrual"></param>
    /// <returns>object of <see cref="ExceptionModel"/></returns>
    private ExceptionModel Accrual(TUser? item, decimal amountAccrual)
    {
        // Check if the user's bank account and bank exist
        if (item?.Card?.BankAccount?.Bank is null)
            return ExceptionModel.EntityIsNull;

        // Check if the user's bank account is valid
        CheckBankAccount(item.Card.BankAccount);

        // Create a new operation object with the necessary information
        var operation = new Operation
        {
            BankID = item.Card.BankAccount.Bank.ID,
            SenderID = item.Card.BankAccount.Bank.ID,
            ReceiverID = item.ID,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Accrual
        };

        // Create the operation in the database
        var createOperation = _bankContext.CreateOperation(operation, OperationKind.Accrual);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        // Perform the accrual operation on the user's bank account
        var accrualOperation = _bankRepository.BankAccountAccrual(item, operation);
        if (accrualOperation != ExceptionModel.Ok)
            return accrualOperation;

        // Return success status
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// asynchronously accrual money on account with the same user id
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amountAccrual"></param>
    /// <returns>object of <see cref="ExceptionModel"/></returns>
    private async Task<ExceptionModel> AccrualAsync(TUser? item, decimal amountAccrual)
    {
        // Check if the user's bank account and bank exist
        if (item?.Card?.BankAccount?.Bank is null)
            return ExceptionModel.EntityIsNull;

        // Check if the user's bank account is valid
        CheckBankAccount(item.Card.BankAccount);

        // Create a new operation object with the necessary information
        var operation = new Operation
        {
            BankID = item.Card.BankAccount.Bank.ID,
            SenderID = item.Card.BankAccount.Bank.ID,
            ReceiverID = item.ID,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Accrual
        };

        // Create the operation in the database
        var createOperation = _bankContext.CreateOperation(operation, OperationKind.Accrual);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        // Perform the accrual operation on the user's bank account
        var accrualOperation = await _bankRepository.BankAccountAccrualAsync(item, operation);
        if (accrualOperation != ExceptionModel.Ok)
            return accrualOperation;

        // Return success status
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// withdraw money from account with the same user id
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amountAccrual"></param>
    /// <returns>object of <see cref="ExceptionModel"/></returns>
    private ExceptionModel Withdraw(TUser? item, decimal amountAccrual)
    {
        // Check if the bank account of the item is null using the null-conditional operator
        if (item?.Card?.BankAccount?.Bank is null)
            return ExceptionModel.EntityIsNull;

        // Call a method to check if the bank account is valid
        CheckBankAccount(item.Card.BankAccount);

        // Create a new operation object with relevant information
        var operation = new Operation
        {
            BankID = item.Card.BankAccount.Bank.ID,
            SenderID = item.ID,
            ReceiverID = item.Card.BankAccount.Bank.ID,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Withdraw
        };

        // Call a method to create the operation in the bank context and check for any errors
        var createOperation = _bankContext.CreateOperation(operation, OperationKind.Withdraw);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        // Call a method to withdraw the specified amount from the bank account and check for any errors
        var withdraw = _bankRepository.BankAccountWithdraw(item, operation);
        if (withdraw != ExceptionModel.Ok)
            return withdraw;

        // Return a success status if all operations were Ok
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// withdraw money from account with the same user id
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amountAccrual"></param>
    /// <returns>object of <see cref="ExceptionModel"/></returns>
    private async Task<ExceptionModel> WithdrawAsync(TUser? item, decimal amountAccrual)
    {
        // Check if the bank account of the item is null using the null-conditional operator
        if (item?.Card?.BankAccount?.Bank is null)
            return ExceptionModel.EntityIsNull;

        // Call a method to check if the bank account is valid
        CheckBankAccount(item.Card.BankAccount);

        // Create a new operation object with relevant information
        var operation = new Operation
        {
            BankID = item.Card.BankAccount.Bank.ID,
            SenderID = item.ID,
            ReceiverID = item.Card.BankAccount.Bank.ID,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Withdraw
        };

        // Call a method to create the operation in the bank context and check for any errors
        var createOperation = _bankContext.CreateOperation(operation, OperationKind.Withdraw);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        // Call a method to withdraw the specified amount from the bank account and check for any errors
        var withdraw = await _bankRepository.BankAccountWithdrawAsync(item, operation);
        if (withdraw != ExceptionModel.Ok)
            return withdraw;

        // Return a success status if all operations were Ok
        return ExceptionModel.Ok;
    }

    public ExceptionModel Create(TBankAccount item)
    {
        if (item is null || Exist(x => x.ID == item.ID) || item.Bank is null)
            return ExceptionModel.EntityIsNull;
        _bankContext.BankAccounts.Add(item);
        _bankContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> CreateAsync(TBankAccount item)
    {
        if (item is null || Exist(x => x.ID == item.ID) || item.Bank is null)
            return ExceptionModel.EntityIsNull;
        _bankContext.BankAccounts.Add(item);
        await _bankContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// updates only BankAccount. Referenced user won't be changed
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public ExceptionModel Update(TBankAccount item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.BankAccounts.Update(item);
        _bankContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> UpdateAsync(TBankAccount item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.BankAccounts.Update(item);
        await _bankContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// removes bank account of user from database
    /// </summary>
    /// <param name="item"></param>
    /// <returns>object of <see cref="ExceptionModel"/></returns>
    public ExceptionModel Delete(TBankAccount item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.BankAccounts.Remove(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> DeleteAsync(TBankAccount item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.BankAccounts.Remove(item);
        await _applicationContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    public TBankAccount Get(Expression<Func<TBankAccount, bool>> predicate)
    {
        return All.FirstOrDefault(predicate) ?? (TBankAccount)BankAccount.Default;
    }

    public async Task<TBankAccount> GetAsync(Expression<Func<TBankAccount, bool>> predicate)
    {
        return await All.FirstOrDefaultAsync(predicate) ?? (TBankAccount)BankAccount.Default;
    }

    public TBankAccount GetWithTracking(Expression<Func<TBankAccount, bool>> predicate)
    {
        return AllWithTracking.FirstOrDefault(predicate) ?? (TBankAccount)BankAccount.Default;
    }

    public bool Exist(Expression<Func<TBankAccount, bool>> predicate)
    {
        return All.Any(predicate);
    }

    public async Task<bool> ExistAsync(Expression<Func<TBankAccount, bool>> predicate)
    {
        return await All.AnyAsync(predicate);
    }

    public bool ExistWithTracking(Expression<Func<TBankAccount, bool>> predicate)
    {
        return AllWithTracking.Any(predicate);
    }

    public bool FitsConditions(TBankAccount? item)
    {
        return item?.Bank is not null && Exist(x => x.ID == item.ID);
    }

    public async Task<bool> FitsConditionsAsync(TBankAccount? item)
    {
        return item?.Bank is not null && await ExistAsync(x => x.ID == item.ID);
    }

    /// <summary>
    /// <para>
    /// The method checks if the BankAccount object is null and throws an exception if it is.
    /// </para>
    /// 
    /// <para>
    /// The next condition checks if the UserID property of the BankAccount object exists in the ID property of any user 
    /// in the Users table of the database. If it doesn't exist, the method throws a KeyNotFoundException with a message indicating 
    /// that the entity of the user with the given ID was not found.
    /// </para>
    /// 
    /// <para>
    /// The last condition checks if there exists a BankAccount object in the database with the same ID as the one passed to the method. 
    /// If it doesn't exist, the method throws a KeyNotFoundException with a message indicating that the entity of the bank 
    /// with the given ID was not found.
    /// </para>
    /// 
    /// </summary>
    /// <param name="item">The item.</param>
    /// <exception cref="ArgumentNullException">Instance of BankAccount</exception>
    /// <exception cref="KeyNotFoundException">
    /// Entity of user with id {{{item.ID}}} wasn't found.
    /// or
    /// Entity of bank with id {{{item.ID}}} wasn't found
    /// </exception>
    private void CheckBankAccount(BankAccount item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (!_applicationContext.Users.AsNoTracking().Select(x => x.ID).Any(x => x == item.UserID))
            throw new KeyNotFoundException($"Entity of user with id {{{item.ID}}} wasn't found.");

        if (!Exist(x => x.ID == item.ID))
            throw new KeyNotFoundException($"Entity of bank with id {{{item.ID}}} wasn't found");
    }

    /// <summary>
    /// It sets the bank context and application context for the bank services using the generic types 
    /// <typeparamref name="TUser"/>
    /// <typeparamref name="TCard"/>
    /// <typeparamref name="TBankAccount"/>
    /// <typeparamref name="TBank"/> and
    /// <typeparamref name="TCredit"/>.
    /// The bank context and application context are set to the values of the private fields 
    /// _bankContext and _applicationContext, respectively. This method is likely called during the initialization of the bank services.
    /// </summary>
    private void SetBankServicesOptions()
    {
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.BankContext = _bankContext;
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext = _applicationContext;
    }

    /// <summary>
    /// <para>
    /// The method returns a boolean value. Inside the method, there is a single line of code that checks 
    /// if the bank associated with the <paramref name="from"/> user's card's bank account is different from the bank associated 
    /// with the <paramref name="to"/> user's card's bank account. 
    /// </para>
    /// 
    /// <para>
    /// If the banks are different, the method returns true. Otherwise, it returns false. 
    /// </para>
    /// 
    /// </summary>
    /// <param name="from">The parameter represents the user who is transferring the money</param>
    /// <param name="to">The parameter represents the user who is receiving the money</param>
    /// <returns></returns>
    private bool AnotherBankTransactionOperation(TUser from, TUser to)
    {
        return from.Card.BankAccount.Bank != to.Card.BankAccount.Bank;
    }

    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    private void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;
        if (disposing)
        {
            _bankContext.Dispose();
            _bankRepository.Dispose();
            _applicationContext.Dispose();
        }
        _disposedValue = true;
    }

    ~BankAccountRepository()
    {
        Dispose(false);
    }
}