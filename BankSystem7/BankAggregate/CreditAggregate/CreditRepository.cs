using System.Data;
using BankSystem7.ApplicationAggregate.Data;
using BankSystem7.ApplicationAggregate.Options;
using BankSystem7.BankAggregate.OperationAggregate;
using BankSystem7.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.BankAggregate.CreditAggregate;

public sealed class CreditRepository(BankContext bankContext, ApplicationContext applicationContext) : ICreditRepository
{
    private bool _disposedValue;

    public IQueryable<Credit> Entities =>
        applicationContext.Credits
            .Include(x => x.Bank)
            .Include(x => x.User)
            .AsNoTracking() ?? Enumerable.Empty<Credit>().AsQueryable();

    public IQueryable<Credit> EntitiesTracking =>
        applicationContext.Credits
            .Include(x => x.Bank)
            .Include(x => x.User) ?? Enumerable.Empty<Credit>().AsQueryable();

    /// <summary>
    /// gives to user credit with the definite amount of money
    /// adds to the table field with credit's data of user
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <returns></returns>
    public ExceptionModel TakeCredit(User? user, Credit? credit)
    {
        if (user?.Card?.BankAccount?.Bank is null
            || credit is null
            || Exist(new(predicate: x => x.Id == credit.Id || x.UserId == user.Id)))
            return ExceptionModel.EntityIsNull;

        var operationAccrualToUserAccount = new Operation(Guid.NewGuid())
        {
            BankId = credit.BankId,
            ReceiverId = credit.UserId,
            SenderId = credit.BankId,
            TransferAmount = credit.CreditAmount
        };
        using var transaction = applicationContext.Database.BeginTransaction(IsolationLevel.Serializable);

        var updateAccountsAfterTakeCredit = UpdateAccountsAfterTakeCredit(user, operationAccrualToUserAccount);
        if (updateAccountsAfterTakeCredit != ExceptionModel.Ok)
        {
            transaction.Rollback();
            return updateAccountsAfterTakeCredit;
        }

        var updateCreditStateAfterTakeCredit = UpdateCreditStateAfterTakeCredit(credit);
        if (updateCreditStateAfterTakeCredit != ExceptionModel.Ok)
        {
            transaction.Rollback();
            return updateCreditStateAfterTakeCredit;
        }

        transaction.Commit();

        return ExceptionModel.Ok;
    }

    /// <summary>
    /// gives to user credit with the definite amount of money
    /// adds to the table field with credit's data of user
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <returns></returns>
    public async Task<ExceptionModel> TakeCreditAsync(User? user, Credit? credit, CancellationToken cancellationToken = default)
    {
        if (user?.Card?.BankAccount?.Bank is null
            || credit is null
            || await ExistAsync(new(predicate: x => x.Id == credit.Id || x.UserId == user.Id), cancellationToken))
            return ExceptionModel.EntityIsNull;

        var operationAccrualToUserAccount = new Operation(Guid.NewGuid())
        {
            BankId = credit.BankId,
            ReceiverId = credit.UserId,
            SenderId = credit.BankId,
            TransferAmount = credit.CreditAmount
        };
        await using var transaction = await applicationContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken: cancellationToken);

        var updateAccountsAfterTakeCredit = await UpdateAccountsAfterTakeCreditAsync(user, operationAccrualToUserAccount, cancellationToken);
        if (updateAccountsAfterTakeCredit != ExceptionModel.Ok)
        {
            await transaction.RollbackAsync(cancellationToken);
            return updateAccountsAfterTakeCredit;
        }

        var updateCreditStateAfterTakeCredit = await UpdateCreditStateAfterTakeCreditAsync(credit, cancellationToken);
        if (updateCreditStateAfterTakeCredit != ExceptionModel.Ok)
        {
            await transaction.RollbackAsync(cancellationToken);
            return updateCreditStateAfterTakeCredit;
        }

        await transaction.CommitAsync(cancellationToken);

        return ExceptionModel.Ok;
    }

    /// <summary>
    /// implements paying some amount of credit for full its repaying
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <param name="payAmount">amount of money for paying</param>
    /// <returns></returns>
    public ExceptionModel PayCredit(User? user, Credit credit, decimal payAmount)
    {
        if (user?.Card?.BankAccount?.Bank is null
            || credit is null
            || !Exist(new(predicate: x => x.Id == credit.Id || x.UserId == user.Id)))
            return ExceptionModel.EntityIsNull;

        var operationWithdrawFromUserAccount = new Operation(Guid.NewGuid())
        {
            BankId = credit.BankId,
            ReceiverId = credit.BankId,
            SenderId = credit.UserId,
            TransferAmount = payAmount
        };

        using var transaction = applicationContext.Database.BeginTransaction(IsolationLevel.Serializable);

        var updateAccountsAfterPayCredit = UpdateAccountsAfterPayCredit(user, credit, operationWithdrawFromUserAccount, payAmount);
        if (updateAccountsAfterPayCredit != ExceptionModel.Ok)
        {
            transaction.Rollback();
            return updateAccountsAfterPayCredit;
        }

        var upd = UpdateCreditStateAfterPayCredit(credit, operationWithdrawFromUserAccount);
        if (upd != ExceptionModel.Ok)
        {
            transaction.Rollback();
            return upd;
        }

        transaction.Commit();

        return ExceptionModel.Ok;
    }

    /// <summary>
    /// implements paying some amount of credit for full its repaying
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <param name="payAmount">amount of money for paying</param>
    /// <returns></returns>
    public async Task<ExceptionModel> PayCreditAsync(User? user, Credit credit, decimal payAmount, CancellationToken cancellationToken = default)
    {
        if (user?.Card?.BankAccount?.Bank is null
            || credit is null
            || !await ExistAsync(new(predicate: x => x.Id == credit.Id || x.UserId == user.Id), cancellationToken))
            return ExceptionModel.EntityIsNull;

        var operationWithdrawFromUserAccount = new Operation(Guid.NewGuid())
        {
            BankId = credit.BankId,
            ReceiverId = credit.BankId,
            SenderId = credit.UserId,
            TransferAmount = payAmount
        };

        await using var transaction = await applicationContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var updateAccountsAfterPayCredit = await UpdateAccountsAfterPayCreditAsync(user, credit, operationWithdrawFromUserAccount, payAmount, cancellationToken);
        if (updateAccountsAfterPayCredit != ExceptionModel.Ok)
        {
            await transaction.RollbackAsync(cancellationToken);
            return updateAccountsAfterPayCredit;
        }

        var upd = await UpdateCreditStateAfterPayCreditAsync(credit, operationWithdrawFromUserAccount, cancellationToken);
        if (upd != ExceptionModel.Ok)
        {
            await transaction.RollbackAsync(cancellationToken);
            return upd;
        }

        await transaction.CommitAsync(cancellationToken);

        return ExceptionModel.Ok;
    }

    public ExceptionModel Create(Credit item)
    {
        if (item is null)
            return ExceptionModel.EntityIsNull;

        if (Exist(new(item.Id)))
            return ExceptionModel.OperationFailed;

        item.User = null;
        item.Bank = null;
        applicationContext.Credits.Add(item);
        applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> CreateAsync(Credit item, CancellationToken cancellationToken = default)
    {
        if (item is null)
            return ExceptionModel.EntityIsNull;

        if (await ExistAsync(new(item.Id), cancellationToken))
            return ExceptionModel.OperationFailed;

        item.User = null;
        item.Bank = null;
        applicationContext.Credits.Add(item);
        await applicationContext.SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    public ExceptionModel Update(Credit item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        applicationContext.Credits.Update(item);
        applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> UpdateAsync(Credit item, CancellationToken cancellationToken = default)
    {
        if (!await FitsConditionsAsync(item, cancellationToken))
            return ExceptionModel.OperationFailed;

        applicationContext.Credits.Update(item);
        await applicationContext.SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    public ExceptionModel Delete(Credit item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        applicationContext.Credits.Remove(item);
        applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> DeleteAsync(Credit item, CancellationToken cancellationToken = default)
    {
        if (!await FitsConditionsAsync(item, cancellationToken))
            return ExceptionModel.OperationFailed;

        applicationContext.Credits.Remove(item);
        await applicationContext.SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    public Credit Get(EntityFindOptions<Credit> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return Credit.Default;

        return Entities.FirstOrDefault(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId))
            ?? Credit.Default;
    }

    public async Task<Credit> GetAsync(EntityFindOptions<Credit> entityFindOptions, CancellationToken cancellationToken = default)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return Credit.Default;

        return await Entities.FirstOrDefaultAsync(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId), cancellationToken)
            ?? Credit.Default;
    }

    public Credit GetTracking(EntityFindOptions<Credit> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return Credit.Default;

        return EntitiesTracking.FirstOrDefault(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId))
            ?? Credit.Default;
    }

    public bool Exist(EntityFindOptions<Credit> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return Entities.Any(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId));
    }

    public async Task<bool> ExistAsync(EntityFindOptions<Credit> entityFindOptions, CancellationToken cancellationToken = default)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return await Entities.AnyAsync(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId), cancellationToken);
    }

    public bool ExistTracking(EntityFindOptions<Credit> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return EntitiesTracking.Any(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId));
    }

    public bool FitsConditions(Credit? item)
    {
        return item is not null && Exist(new(item.Id));
    }

    public async Task<bool> FitsConditionsAsync(Credit? item, CancellationToken cancellationToken = default)
    {
        return item is not null && await ExistAsync(new(item.Id), cancellationToken);
    }

    private ExceptionModel UpdateCreditStateAfterPayCredit(Credit credit, Operation operationWithdrawFromUserAccount)
    {
        if (credit.RepaymentAmount == operationWithdrawFromUserAccount.TransferAmount)
            return Delete(credit);
        else
        {
            credit.RepaymentAmount -= operationWithdrawFromUserAccount.TransferAmount;
            return Update(credit);
        }
    }

    private async Task<ExceptionModel> UpdateCreditStateAfterPayCreditAsync(Credit credit, Operation operationWithdrawFromUserAccount, CancellationToken cancellationToken = default)
    {
        if (credit.RepaymentAmount == operationWithdrawFromUserAccount.TransferAmount)
            return await DeleteAsync(credit, cancellationToken);
        else
        {
            credit.RepaymentAmount -= operationWithdrawFromUserAccount.TransferAmount;
            return await UpdateAsync(credit, cancellationToken);
        }
    }

    private ExceptionModel UpdateCreditStateAfterTakeCredit(Credit credit, CancellationToken cancellationToken = default)
    {
        var createCredit = Create(credit);
        if (createCredit != ExceptionModel.Ok)
            return createCredit;

        return ExceptionModel.Ok;
    }

    private async Task<ExceptionModel> UpdateCreditStateAfterTakeCreditAsync(Credit credit, CancellationToken cancellationToken = default)
    {
        var createCredit = await CreateAsync(credit, cancellationToken);
        if (createCredit != ExceptionModel.Ok)
            return createCredit;

        return ExceptionModel.Ok;
    }

    private ExceptionModel UpdateAccountsAfterTakeCredit(User user, Operation operationAccrualToUserAccount)
    {
        // here creates operation for accrual money on user bank account
        var createOperation = bankContext.CreateOperation(operationAccrualToUserAccount, OperationKind.Accrual);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        // accrual money to user's bank account
        var accrualOperation = bankContext.BankAccountAccrual(user, user.Card?.BankAccount?.Bank, operationAccrualToUserAccount);
        if (accrualOperation != ExceptionModel.Ok)
            return accrualOperation;

        return ExceptionModel.Ok;
    }

    private async Task<ExceptionModel> UpdateAccountsAfterTakeCreditAsync(User user, Operation operationAccrualToUserAccount, CancellationToken cancellationToken = default)
    {
        // here creates operation for accrual money on user bank account
        var createOperation = await bankContext.CreateOperationAsync(operationAccrualToUserAccount, OperationKind.Accrual, cancellationToken);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        // accrual money to user's bank account
        var accrualOperation = await bankContext.BankAccountAccrualAsync(user, user.Card?.BankAccount?.Bank, operationAccrualToUserAccount, cancellationToken);
        if (accrualOperation != ExceptionModel.Ok)
            return accrualOperation;

        return ExceptionModel.Ok;
    }

    private ExceptionModel UpdateAccountsAfterPayCredit(User user, Credit credit, Operation operationWithdrawFromUserAccount, decimal payAmount)
    {
        if (payAmount > credit.RepaymentAmount)
            operationWithdrawFromUserAccount.TransferAmount = credit.RepaymentAmount;

        // here creates operation for accrual money on user bank account
        var createOperation = bankContext.CreateOperation(operationWithdrawFromUserAccount, OperationKind.Withdraw);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        // withdraw money to user's bank account
        var bankAccountWithdraw = bankContext.BankAccountWithdraw(user, user.Card?.BankAccount?.Bank, operationWithdrawFromUserAccount);
        if (bankAccountWithdraw != ExceptionModel.Ok)
            return bankAccountWithdraw;
        return ExceptionModel.Ok;
    }

    private async Task<ExceptionModel> UpdateAccountsAfterPayCreditAsync(User user, Credit credit, Operation operationWithdrawFromUserAccount, decimal payAmount, CancellationToken cancellationToken = default)
    {
        if (payAmount > credit.RepaymentAmount)
            operationWithdrawFromUserAccount.TransferAmount = credit.RepaymentAmount;

        // here creates operation for accrual money on user bank account
        var createOperation = await bankContext.CreateOperationAsync(operationWithdrawFromUserAccount, OperationKind.Withdraw, cancellationToken);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        // withdraw money to user's bank account
        var bankAccountWithdraw = await bankContext.BankAccountWithdrawAsync(user, user.Card?.BankAccount?.Bank, operationWithdrawFromUserAccount, cancellationToken);
        if (bankAccountWithdraw != ExceptionModel.Ok)
            return bankAccountWithdraw;
        return ExceptionModel.Ok;
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