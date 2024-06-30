using System.Data;
using BankSystem7.ApplicationAggregate.Data;
using BankSystem7.ApplicationAggregate.Options;
using BankSystem7.BankAggregate.OperationAggregate;
using BankSystem7.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.BankAggregate.BankAccountAggregate;

public sealed class BankAccountRepository(BankRepository bankRepository,
                                          ApplicationContext applicationContext,
                                          BankContext bankContext) : IBankAccountRepository
{
    private bool _disposedValue;

    public IQueryable<BankAccount> Entities =>
        applicationContext.BankAccounts
            .Include(x => x.Bank)
            .Include(x => x.User.Card)
            .AsNoTracking();

    public IQueryable<BankAccount> EntitiesTracking =>
        applicationContext.BankAccounts
            .Include(x => x.Bank)
            .Include(x => x.User.Card);

    public ExceptionModel Transfer(User? from, User? to, decimal transferAmount)
    {
        // Check if the sender's bank account, receiver's bank account, and transfer amount are valid. If not, return an operation failed exception.
        if (from?.Card?.BankAccount is null || to?.Card?.BankAccount is null || transferAmount <= 0)
            return ExceptionModel.OperationFailed;

        // Check if the sender's bank account and receiver's bank account exist in the database. If not, return an operation failed exception.
        if (!Exist(new(from.Card.BankAccount.Id))
            || !Exist(new(to.Card.BankAccount.Id)))
            return ExceptionModel.OperationFailed;

        // Begin a new transaction with repeatable read isolation level.
        using var transaction = bankContext.Database.BeginTransaction(IsolationLevel.RepeatableRead);

        // Check bank where transaction was sent
        bankRepository.AnotherBankTransactionOperation = AnotherBankTransactionOperation(from, to);

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

    public async Task<ExceptionModel> TransferAsync(User? from, User? to, decimal transferAmount, CancellationToken cancellationToken = default)
    {
        // Check if the sender's bank account, receiver's bank account, and transfer amount are valid. If not, return an operation failed exception.
        if (from?.Card?.BankAccount is null || to?.Card?.BankAccount is null || transferAmount <= 0)
            return ExceptionModel.OperationFailed;

        // Check if the sender's bank account and receiver's bank account exist in the database. If not, return an operation failed exception.
        if (!await ExistAsync(new(from.Card.BankAccount.Id), cancellationToken)
            || !await ExistAsync(new(to.Card.BankAccount.Id), cancellationToken))
            return ExceptionModel.OperationFailed;

        // Begin a new transaction with repeatable read isolation level.
        using var transaction = await bankContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);

        // Check bank where transaction was sent
        bankRepository.AnotherBankTransactionOperation = AnotherBankTransactionOperation(from, to);

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
    private ExceptionModel Accrual(User? item, decimal amountAccrual)
    {
        // Check if the user's bank account and bank exist
        if (item?.Card?.BankAccount?.Bank is null)
            return ExceptionModel.EntityIsNull;

        // Check if the user's bank account is valid
        CheckBankAccount(item.Card.BankAccount);

        // Create a new operation object with the necessary information
        var operation = new Operation(Guid.NewGuid())
        {
            BankId = item.Card.BankAccount.Bank.Id,
            SenderId = item.Card.BankAccount.Bank.Id,
            ReceiverId = item.Id,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Accrual
        };

        // Create the operation in the database
        var createOperation = bankContext.CreateOperation(operation, OperationKind.Accrual);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        // Perform the accrual operation on the user's bank account
        var accrualOperation = bankRepository.BankAccountAccrual(item, operation);
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
    private async Task<ExceptionModel> AccrualAsync(User? item, decimal amountAccrual, CancellationToken cancellationToken = default)
    {
        // Check if the user's bank account and bank exist
        if (item?.Card?.BankAccount?.Bank is null)
            return ExceptionModel.EntityIsNull;

        // Check if the user's bank account is valid
        CheckBankAccount(item.Card.BankAccount);

        // Create a new operation object with the necessary information
        var operation = new Operation(Guid.NewGuid())
        {
            BankId = item.Card.BankAccount.Bank.Id,
            SenderId = item.Card.BankAccount.Bank.Id,
            ReceiverId = item.Id,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Accrual
        };

        // Create the operation in the database
        var createOperation = bankContext.CreateOperation(operation, OperationKind.Accrual);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        // Perform the accrual operation on the user's bank account
        var accrualOperation = await bankRepository.BankAccountAccrualAsync(item, operation);
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
    private ExceptionModel Withdraw(User? item, decimal amountAccrual)
    {
        // Check if the bank account of the item is null using the null-conditional operator
        if (item?.Card?.BankAccount?.Bank is null)
            return ExceptionModel.EntityIsNull;

        // Call a method to check if the bank account is valid
        CheckBankAccount(item.Card.BankAccount);

        // Create a new operation object with relevant information
        var operation = new Operation(Guid.NewGuid())
        {
            BankId = item.Card.BankAccount.Bank.Id,
            SenderId = item.Id,
            ReceiverId = item.Card.BankAccount.Bank.Id,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Withdraw
        };

        // Call a method to create the operation in the bank context and check for any errors
        var createOperation = bankContext.CreateOperation(operation, OperationKind.Withdraw);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        // Call a method to withdraw the specified amount from the bank account and check for any errors
        var withdraw = bankRepository.BankAccountWithdraw(item, operation);
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
    private async Task<ExceptionModel> WithdrawAsync(User? item, decimal amountAccrual, CancellationToken cancellationToken = default)
    {
        // Check if the bank account of the item is null using the null-conditional operator
        if (item?.Card?.BankAccount?.Bank is null)
            return ExceptionModel.EntityIsNull;

        // Call a method to check if the bank account is valid
        CheckBankAccount(item.Card.BankAccount);

        // Create a new operation object with relevant information
        var operation = new Operation(Guid.NewGuid())
        {
            BankId = item.Card.BankAccount.Bank.Id,
            SenderId = item.Id,
            ReceiverId = item.Card.BankAccount.Bank.Id,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Withdraw
        };

        // Call a method to create the operation in the bank context and check for any errors
        var createOperation = bankContext.CreateOperation(operation, OperationKind.Withdraw);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        // Call a method to withdraw the specified amount from the bank account and check for any errors
        var withdraw = await bankRepository.BankAccountWithdrawAsync(item, operation);
        if (withdraw != ExceptionModel.Ok)
            return withdraw;

        // Return a success status if all operations were Ok
        return ExceptionModel.Ok;
    }

    public ExceptionModel Create(BankAccount item)
    {
        if (item is null || Exist(new(item.Id)) || item.Bank is null)
            return ExceptionModel.EntityIsNull;
        bankContext.BankAccounts.Add(item);
        bankContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> CreateAsync(BankAccount item, CancellationToken cancellationToken = default)
    {
        if (item is null || Exist(new(item.Id)) || item.Bank is null)
            return ExceptionModel.EntityIsNull;
        bankContext.BankAccounts.Add(item);
        await bankContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// updates only BankAccount. Referenced user won't be changed
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public ExceptionModel Update(BankAccount item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        applicationContext.BankAccounts.Update(item);
        bankContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> UpdateAsync(BankAccount item, CancellationToken cancellationToken = default)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        applicationContext.BankAccounts.Update(item);
        await bankContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// removes bank account of user from database
    /// </summary>
    /// <param name="item"></param>
    /// <returns>object of <see cref="ExceptionModel"/></returns>
    public ExceptionModel Delete(BankAccount item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        applicationContext.BankAccounts.Remove(item);
        applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> DeleteAsync(BankAccount item, CancellationToken cancellationToken = default)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        applicationContext.BankAccounts.Remove(item);
        await applicationContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    public BankAccount Get(EntityFindOptions<BankAccount> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return BankAccount.Default;

        return Entities.FirstOrDefault(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId))
            ?? BankAccount.Default;
    }

    public async Task<BankAccount> GetAsync(EntityFindOptions<BankAccount> entityFindOptions, CancellationToken cancellationToken = default)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return BankAccount.Default;

        return await Entities.FirstOrDefaultAsync(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId),
            cancellationToken) ?? BankAccount.Default;
    }

    public BankAccount GetTracking(EntityFindOptions<BankAccount> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return BankAccount.Default;

        return EntitiesTracking.FirstOrDefault(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId))
            ?? BankAccount.Default;
    }

    public bool Exist(EntityFindOptions<BankAccount> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return Entities.Any(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId));
    }

    public async Task<bool> ExistAsync(EntityFindOptions<BankAccount> entityFindOptions, CancellationToken cancellationToken = default)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return await Entities.AnyAsync(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId), cancellationToken);
    }

    public bool ExistTracking(EntityFindOptions<BankAccount> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return EntitiesTracking.Any(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId));
    }

    public bool FitsConditions(BankAccount? item)
    {
        return item?.Bank is not null && Exist(new(item.Id));
    }

    public async Task<bool> FitsConditionsAsync(BankAccount? item, CancellationToken cancellationToken = default)
    {
        return item?.Bank is not null && await ExistAsync(new(item.Id), cancellationToken);
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

        if (!applicationContext.Users.AsNoTracking().Select(x => x.Id).Any(x => x == item.UserId))
            throw new KeyNotFoundException($"Entity of user with id {{{item.Id}}} wasn't found.");

        if (!Exist(new(item.Id)))
            throw new KeyNotFoundException($"Entity of bank with id {{{item.Id}}} wasn't found");
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
    private bool AnotherBankTransactionOperation(User from, User to)
    {
        return from.Card.BankAccount.Bank != to.Card.BankAccount.Bank;
    }

    public void Dispose()
    {
        if (_disposedValue)
            return;

        bankContext?.Dispose();
        bankRepository?.Dispose();
        applicationContext?.Dispose();

        _disposedValue = true;
    }
}