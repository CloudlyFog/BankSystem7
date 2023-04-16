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
            return ExceptionModel.VariableIsNull;

        if (Exist(x => x.ID == item.ID))
            return ExceptionModel.OperationFailed;

        item.User = null;
        item.Bank = null;
        _applicationContext.Credits.Add(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Update(TCredit item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Credits.Update(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(TCredit item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Credits.Remove(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public IEnumerable<TCredit> All =>
        _applicationContext.Credits
        .Include(x => x.Bank).Include(x => x.User)
        .AsNoTracking() ?? Enumerable.Empty<TCredit>();

    public TCredit Get(Func<TCredit, bool> predicate)
    {
        return _applicationContext.Credits
        .Include(x => x.Bank).Include(x => x.User)
        .AsNoTracking().AsEnumerable()
        .FirstOrDefault(predicate) ?? (TCredit)Credit.Default;
    }

    public bool Exist(Func<TCredit, bool> predicate)
    {
        return _applicationContext.Credits
            .Include(x => x.Bank)
            .Include(x => x.User)
            .AsNoTracking().AsEnumerable()
            .Any(predicate);
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
    public ExceptionModel TakeCredit(User? user, TCredit? credit)
    {
        if (user.Card?.BankAccount?.Bank is null || credit is null || Exist(x => x.ID == credit.ID || x.UserID == user.ID))
            return ExceptionModel.VariableIsNull;

        var operationAccrualOnUserAccount = new Operation
        {
            BankID = credit.BankID,
            ReceiverID = credit.UserID,
            SenderID = credit.BankID,
            TransferAmount = credit.CreditAmount
        };
        using var transaction = _applicationContext.Database.BeginTransaction(IsolationLevel.Serializable);

        if (_bankContext.CreateOperation(operationAccrualOnUserAccount, OperationKind.Accrual) !=
            ExceptionModel.Successfully)
            return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

        // accrual money to user's bank account
        if (_bankContext.BankAccountAccrual(user, user.Card?.BankAccount?.Bank, operationAccrualOnUserAccount) != ExceptionModel.Successfully)
            return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

        if (Create(credit) != ExceptionModel.Successfully)
            return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

        transaction.Commit();

        return ExceptionModel.Successfully;
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
        if (user.Card?.BankAccount?.Bank is null || credit is null || !Exist(x => x.ID == credit.ID || x.UserID == user.ID))
            return ExceptionModel.VariableIsNull;

        var operationAccrualOnUserAccount = new Operation()
        {
            BankID = credit.BankID,
            ReceiverID = credit.UserID,
            SenderID = credit.BankID,
            TransferAmount = payAmount
        };

        if (payAmount > credit.RepaymentAmount)
            operationAccrualOnUserAccount.TransferAmount = credit.RepaymentAmount;

        using var transaction = _applicationContext.Database.BeginTransaction(IsolationLevel.Serializable);

        if (_bankContext.CreateOperation(operationAccrualOnUserAccount, OperationKind.Accrual) !=
            ExceptionModel.Successfully) // here creates operation for accrual money on user bank account
            return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

        // withdraw money to user's bank account
        if (_bankContext.BankAccountWithdraw(user, user.Card?.BankAccount?.Bank, operationAccrualOnUserAccount) != ExceptionModel.Successfully)
            return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

        if (credit.RepaymentAmount == operationAccrualOnUserAccount.TransferAmount)
        {
            if (Delete(credit) != ExceptionModel.Successfully)
                return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();
        }
        else
        {
            credit.RepaymentAmount -= operationAccrualOnUserAccount.TransferAmount;
            if (Update(credit) != ExceptionModel.Successfully)
                return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();
        }

        transaction.Commit();

        return ExceptionModel.Successfully;
    }

    /// <summary>
    /// repays user's credit
    /// removes from the table field with credit's data of user
    /// </summary>
    /// <param name="user">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <returns></returns>
    [Obsolete("This method is using for repaying a credit. " +
              "Instead of it you can use new method PayCredit that takes as third arg a value on which credit will decrease.")]
    public ExceptionModel RepayCredit(TUser? user, TCredit? credit) =>
        PayCredit(user, credit, credit.CreditAmount);

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