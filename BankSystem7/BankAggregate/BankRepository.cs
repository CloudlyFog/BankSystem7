using BankSystem7.ApplicationAggregate.Data;
using BankSystem7.ApplicationAggregate.Options;
using BankSystem7.BankAggregate.OperationAggregate;
using BankSystem7.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.BankAggregate;

public sealed class BankRepository(BankContext bankContext, ApplicationContext applicationContext) : IBankRepository
{
    private bool _disposedValue;

    internal bool AnotherBankTransactionOperation { get; set; }

    public IQueryable<Bank> Entities =>
        applicationContext.Banks
            .Include(x => x.BankAccounts)
            .Include(x => x.Credits)
            .AsNoTracking();

    public IQueryable<Bank> EntitiesTracking =>
        applicationContext.Banks
            .Include(x => x.BankAccounts)
            .Include(x => x.Credits);

    /// <summary>
    /// accrual money to user bank account from bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public ExceptionModel BankAccountAccrual(User user, Operation operation)
    {
        // Check if the operation status is OK.
        if (operation.OperationStatus is not StatusOperationCode.Ok or StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        // If the transaction is with another bank, subtract the transfer amount from the user's bank account.
        if (AnotherBankTransactionOperation)
            user.Card.BankAccount.Bank.AccountAmount -= operation.TransferAmount;

        // Add the transfer amount to the user's bank account.
        user.Card.BankAccount.BankAccountAmount += operation.TransferAmount;

        // Update the user's card amount to reflect the new bank account amount.
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;

        // Update the user's information in the database.
        bankContext.Update(user);
        bankContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// accrual money to user bank account from bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public async Task<ExceptionModel> BankAccountAccrualAsync(User user, Operation operation, CancellationToken cancellationToken = default)
    {
        // Check if the operation status is OK.
        if (operation.OperationStatus is not StatusOperationCode.Ok or StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        // If the transaction is with another bank, subtract the transfer amount from the user's bank account.
        if (AnotherBankTransactionOperation)
            user.Card.BankAccount.Bank.AccountAmount -= operation.TransferAmount;

        // Add the transfer amount to the user's bank account.
        user.Card.BankAccount.BankAccountAmount += operation.TransferAmount;

        // Update the user's card amount to reflect the new bank account amount.
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;

        // Update the user's information in the database.
        bankContext.Update(user);
        await bankContext.SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// withdraw money from user bank account and accrual to bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public ExceptionModel BankAccountWithdraw(User user, Operation operation)
    {
        // Check if the operation status is OK.
        if (operation.OperationStatus is not StatusOperationCode.Ok or StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        // Check if the user's card's bank account's bank is null or if the bank ID does not exist in the database
        if (user?.Card?.BankAccount?.Bank is null || !Exist(new(user.Card.BankAccount.Bank.Id)))
            return ExceptionModel.EntityIsNull;

        // If it is another bank transaction operation, add the transfer amount to the bank account amount of the user's card's bank
        if (AnotherBankTransactionOperation)
            user.Card.BankAccount.Bank.AccountAmount += operation.TransferAmount;

        // Subtract the transfer amount from the bank account amount of the user's card's bank and update the user's card amount
        user.Card.BankAccount.BankAccountAmount -= operation.TransferAmount;
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;

        // Update the user's information in the database and save changes
        bankContext.Update(user);
        bankContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// withdraw money from user bank account and accrual to bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public async Task<ExceptionModel> BankAccountWithdrawAsync(User user, Operation operation, CancellationToken cancellationToken = default)
    {
        // Check if the operation status is OK.
        if (operation.OperationStatus is not StatusOperationCode.Ok or StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        // Check if the user's card's bank account's bank is null or if the bank ID does not exist in the database
        if (user?.Card?.BankAccount?.Bank is null || !await ExistAsync(new(user.Card.BankAccount.Bank.Id)))
            return ExceptionModel.EntityIsNull;

        // If it is another bank transaction operation, add the transfer amount to the bank account amount of the user's card's bank
        if (AnotherBankTransactionOperation)
            user.Card.BankAccount.Bank.AccountAmount += operation.TransferAmount;

        // Subtract the transfer amount from the bank account amount of the user's card's bank and update the user's card amount
        user.Card.BankAccount.BankAccountAmount -= operation.TransferAmount;
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;

        // Update the user's information in the database and save changes
        bankContext.Update(user);
        await bankContext.SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    public decimal CalculateBankAccountAmount(decimal accountAmountValue)
    {
        return accountAmountValue - 0;
    }

    public ExceptionModel Create(Bank item)
    {
        if (item is null)
            return ExceptionModel.EntityIsNull;

        if (Exist(new(item.Id)))
            return Update(item);

        applicationContext.Add(item);
        applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> CreateAsync(Bank item, CancellationToken cancellationToken = default)
    {
        if (item is null)
            return ExceptionModel.EntityIsNull;

        if (await ExistAsync(new(item.Id), cancellationToken))
            return Update(item);

        applicationContext.Add(item);
        await applicationContext.SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    public ExceptionModel Delete(Bank item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        applicationContext.Remove(item);
        applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> DeleteAsync(Bank item, CancellationToken cancellationToken = default)
    {
        if (!await FitsConditionsAsync(item, cancellationToken))
            return ExceptionModel.OperationFailed;

        applicationContext.Remove(item);
        await applicationContext.SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    public ExceptionModel Update(Bank item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        applicationContext.Banks.Update(item);
        applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> UpdateAsync(Bank item, CancellationToken cancellationToken = default)
    {
        if (!await FitsConditionsAsync(item, cancellationToken))
            return ExceptionModel.OperationFailed;

        applicationContext.Banks.Update(item);
        await applicationContext.SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    public Bank Get(EntityFindOptions<Bank> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return Bank.Default;

        return Entities.FirstOrDefault(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId))
            ?? Bank.Default;
    }

    public async Task<Bank> GetAsync(EntityFindOptions<Bank> entityFindOptions, CancellationToken cancellationToken = default)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return Bank.Default;

        return await Entities.FirstOrDefaultAsync(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId),
            cancellationToken: cancellationToken) ?? Bank.Default;
    }

    public Bank GetTracking(EntityFindOptions<Bank> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return Bank.Default;

        return EntitiesTracking.FirstOrDefault(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId))
            ?? Bank.Default;
    }

    public bool Exist(EntityFindOptions<Bank> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return Entities.Any(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId));
    }

    public async Task<bool> ExistAsync(EntityFindOptions<Bank> entityFindOptions, CancellationToken cancellationToken = default)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;

        return await Entities.AnyAsync(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId),
            cancellationToken: cancellationToken);
    }

    public bool ExistTracking(EntityFindOptions<Bank> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return EntitiesTracking.Any(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId));
    }

    public bool FitsConditions(Bank? item)
    {
        return item is not null && Exist(new(item.Id));
    }

    public async Task<bool> FitsConditionsAsync(Bank? item, CancellationToken cancellationToken = default)
    {
        return item is not null && await ExistAsync(new(item.Id), cancellationToken);
    }

    public void Dispose()
    {
        if (_disposedValue)
            return;

        bankContext?.Dispose();
        applicationContext?.Dispose();

        _disposedValue = true;
    }
}