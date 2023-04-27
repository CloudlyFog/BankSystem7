using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;

namespace BankSystem7.Services.Repositories;

public sealed class CreditRepository<TUser, TCard, TBankAccount, TBank, TCredit> : IRepository<TCredit>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private BankContext<TUser, TCard, TBankAccount, TBank, TCredit> _bankContext;
    private ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> _applicationContext;
    private bool _disposedValue;

    public CreditRepository()
    {
        _bankContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.BankContext ?? new BankContext<TUser, TCard, TBankAccount, TBank, TCredit>(BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
    }

    public CreditRepository(string connection)
    {
        _bankContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.BankContext ?? new BankContext<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
    }

    public ExceptionModel Create(TCredit item)
    {
        if (item is null)
            return ExceptionModel.EntityIsNull;

        if (Exist(x => x.ID == item.ID))
            return ExceptionModel.OperationFailed;

        item.User = null;
        item.Bank = null;
        _applicationContext.Credits.Add(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public ExceptionModel Update(TCredit item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Credits.Update(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public ExceptionModel Delete(TCredit item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Credits.Remove(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public IQueryable<TCredit> All =>
        _applicationContext.Credits
        .Include(x => x.Bank)
        .Include(x => x.User)
        .AsNoTracking() ?? Enumerable.Empty<TCredit>().AsQueryable();

    public TCredit Get(Expression<Func<TCredit, bool>> predicate)
    {
        return All.FirstOrDefault(predicate) ?? (TCredit)Credit.Default;
    }

    public bool Exist(Expression<Func<TCredit, bool>> predicate)
    {
        return All.Any(predicate);
    }

    public bool FitsConditions(TCredit? item)
    {
        return item is not null && Exist(x => x.ID == item.ID);
    }

    /// <summary>
    /// gives to user credit with the definite amount of money
    /// adds to the table field with credit's data of user
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <returns></returns>
    public ExceptionModel TakeCredit(TUser? user, TCredit? credit)
    {
        if (user?.Card?.BankAccount?.Bank is null || credit is null || Exist(x => x.ID == credit.ID || x.UserID == user.ID))
            return ExceptionModel.EntityIsNull;

        var operationAccrualToUserAccount = new Operation
        {
            BankID = credit.BankID,
            ReceiverID = credit.UserID,
            SenderID = credit.BankID,
            TransferAmount = credit.CreditAmount
        };
        using var transaction = _applicationContext.Database.BeginTransaction(IsolationLevel.Serializable);

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
    /// implements paying some amount of credit for full its repaying
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <param name="payAmount">amount of money for paying</param>
    /// <returns></returns>
    public ExceptionModel PayCredit(TUser? user, TCredit credit, decimal payAmount)
    {
        if (user?.Card?.BankAccount?.Bank is null || credit is null || !Exist(x => x.ID == credit.ID || x.UserID == user.ID))
            return ExceptionModel.EntityIsNull;

        var operationWithdrawFromUserAccount = new Operation()
        {
            BankID = credit.BankID,
            ReceiverID = credit.BankID,
            SenderID = credit.UserID,
            TransferAmount = payAmount
        };

        using var transaction = _applicationContext.Database.BeginTransaction(IsolationLevel.Serializable);

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

    private ExceptionModel UpdateCreditStateAfterPayCredit(TCredit credit, Operation operationWithdrawFromUserAccount)
    {

        if (credit.RepaymentAmount == operationWithdrawFromUserAccount.TransferAmount)
            return Delete(credit);
        else
        {
            credit.RepaymentAmount -= operationWithdrawFromUserAccount.TransferAmount;
            return Update(credit);
        }
    }

    private ExceptionModel UpdateCreditStateAfterTakeCredit(TCredit credit)
    {
        var createCredit = Create(credit);
        if (createCredit != ExceptionModel.Ok)
            return createCredit;

        return ExceptionModel.Ok;
    }

    private ExceptionModel UpdateAccountsAfterTakeCredit(TUser user, Operation operationAccrualToUserAccount)
    {
        // here creates operation for accrual money on user bank account
        var createOperation = _bankContext.CreateOperation(operationAccrualToUserAccount, OperationKind.Accrual);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        // accrual money to user's bank account
        var accrualOperation = _bankContext.BankAccountAccrual(user, user.Card?.BankAccount?.Bank, operationAccrualToUserAccount);
        if (accrualOperation != ExceptionModel.Ok)
            return accrualOperation;


        return ExceptionModel.Ok;
    }

    private ExceptionModel UpdateAccountsAfterPayCredit(TUser user, TCredit credit, Operation operationWithdrawFromUserAccount, decimal payAmount)
    {
        if (payAmount > credit.RepaymentAmount)
            operationWithdrawFromUserAccount.TransferAmount = credit.RepaymentAmount;

        // here creates operation for accrual money on user bank account
        var createOperation = _bankContext.CreateOperation(operationWithdrawFromUserAccount, OperationKind.Withdraw);
        if (createOperation != ExceptionModel.Ok) 
            return createOperation;

        // withdraw money to user's bank account
        var bankAccountWithdraw = _bankContext.BankAccountWithdraw(user, user.Card?.BankAccount?.Bank, operationWithdrawFromUserAccount);
        if (bankAccountWithdraw != ExceptionModel.Ok)
            return bankAccountWithdraw;
        return ExceptionModel.Ok;
    }

    private void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;
        if (disposing)
        {
            _bankContext.Dispose();
            _applicationContext.Dispose();
        }

        _bankContext = null;
        _applicationContext = null;
        _disposedValue = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~CreditRepository()
    {
        Dispose(false);
    }
}