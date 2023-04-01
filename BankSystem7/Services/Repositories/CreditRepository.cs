using System.Data;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Repositories;

public class CreditRepository : IRepository<Credit>
{
    private BankContext _bankContext;
    private ApplicationContext _applicationContext;
    private UserRepository _userRepository;
    private bool _disposedValue;

    public CreditRepository()
    {
        _bankContext = BankServicesOptions.BankContext ?? new BankContext(BankServicesOptions.Connection);
        _applicationContext = BankServicesOptions.ApplicationContext ??
                              new ApplicationContext(BankServicesOptions.Connection);
        _userRepository = BankServicesOptions.ServiceConfiguration?.UserRepository ??
                          new UserRepository(BankServicesOptions.Connection);
    }

    public CreditRepository(string connection)
    {
        _bankContext = BankServicesOptions.BankContext ?? new BankContext(connection);
        _applicationContext = BankServicesOptions.ApplicationContext ??
                              new ApplicationContext(connection);
        _userRepository = BankServicesOptions.ServiceConfiguration?.UserRepository ??
                          new UserRepository(connection);
    }

    public ExceptionModel Create(Credit item)
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

    public ExceptionModel Update(Credit item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.Credits.Update(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(Credit item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.Credits.Remove(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public IEnumerable<Credit> All => _applicationContext.Credits.AsNoTracking();
    public Credit Get(Expression<Func<Credit, bool>> predicate) => _applicationContext.Credits.AsNoTracking().FirstOrDefault(predicate);

    public bool Exist(Expression<Func<Credit, bool>> predicate) => _applicationContext.Credits.AsNoTracking().Any(predicate);

    public bool FitsConditions(Credit item)
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
    public ExceptionModel TakeCredit(User? user, Credit? credit)
    {
        if (user is null || credit is null)
            return ExceptionModel.VariableIsNull;

        var operationAccrualOnUserAccount = new Operation()
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
    public ExceptionModel PayCredit(User? user, Credit credit, decimal payAmount)
    {
        if (user is null || credit is null)
            return ExceptionModel.VariableIsNull;

        var operationAccrualOnUserAccount = new Operation()
        {
            BankID = credit.BankID,
            ReceiverID = credit.UserID,
            SenderID = credit.BankID,
            TransferAmount = payAmount
        };

        if (payAmount > credit.CreditAmount)
            operationAccrualOnUserAccount.TransferAmount = credit.CreditAmount;

        using var transaction = _applicationContext.Database.BeginTransaction(IsolationLevel.Serializable);

        if (_bankContext.CreateOperation(operationAccrualOnUserAccount, OperationKind.Accrual) !=
            ExceptionModel.Successfully) // here creates operation for accrual money on user bank account
            return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

        // withdraw money to user's bank account
        if (_bankContext.BankAccountWithdraw(user, user.Card?.BankAccount?.Bank, operationAccrualOnUserAccount) != ExceptionModel.Successfully)
            return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

        if (credit.CreditAmount == operationAccrualOnUserAccount.TransferAmount)
        {
            if (Delete(credit) != ExceptionModel.Successfully)
                return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();
        }

        else
        {
            credit.CreditAmount = operationAccrualOnUserAccount.TransferAmount;
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
    public ExceptionModel RepayCredit(User? user, Credit? credit) =>
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