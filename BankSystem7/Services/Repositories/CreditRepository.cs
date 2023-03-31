using System.Data;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Repositories;

public class CreditRepository : ApplicationContext, IRepository<Credit>
{
    private BankContext _bankContext;

    public CreditRepository()
    {
        _bankContext = BankServicesOptions.BankContext ?? new BankContext(BankServicesOptions.Connection);
    }

    public CreditRepository(string connection)
    {
        _bankContext = BankServicesOptions.BankContext ?? new BankContext(connection);
    }

    public ExceptionModel Create(Credit item)
    {
        if (item is null)
            return ExceptionModel.VariableIsNull;

        if (Exist(x => x.ID == item.ID))
            return ExceptionModel.OperationFailed;

        Credits.Add(item);
        SaveChanges();
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Update(Credit item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        Credits.Update(item);
        SaveChanges();
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(Credit item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        Credits.Remove(item);
        SaveChanges();
        return ExceptionModel.Successfully;
    }

    public IEnumerable<Credit> All => Credits.AsNoTracking();
    public Credit Get(Expression<Func<Credit, bool>> predicate) => Credits.AsNoTracking().FirstOrDefault(predicate);

    public bool Exist(Expression<Func<Credit, bool>> predicate) => Credits.AsNoTracking().Any(predicate);

    public bool FitsConditions(Credit item)
    {
        return item is not null && Exist(x => x.ID == item.ID);
    }

    /// <summary>
    /// gives to user credit with the definite amount of money
    /// adds to the table field with credit's data of user
    /// </summary>
    /// <param name="bankAccount">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <returns></returns>
    public ExceptionModel TakeCredit(BankAccount? bankAccount, Credit? credit)
    {
        if (bankAccount is null || credit is null)
            return ExceptionModel.VariableIsNull;

        var operationAccrualOnUserAccount = new Operation()
        {
            BankID = credit.BankID,
            ReceiverID = credit.UserBankAccountID,
            SenderID = credit.BankID,
            TransferAmount = credit.CreditAmount
        };
        using var transaction = Database.BeginTransaction(IsolationLevel.Serializable);

        if (_bankContext.CreateOperation(operationAccrualOnUserAccount, OperationKind.Accrual) !=
            ExceptionModel.Successfully)
            return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

        // accrual money to user's bank account
        if (_bankContext.BankAccountAccrual(bankAccount,
                Banks.AsNoTracking().FirstOrDefault(x => x.ID == bankAccount.BankID),
                operationAccrualOnUserAccount) != ExceptionModel.Successfully)
            return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

        if (Create(credit) != ExceptionModel.Successfully)
            return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

        transaction.Commit();

        return ExceptionModel.Successfully;
    }

    /// <summary>
    /// implements paying some amount of credit for full its repaying
    /// </summary>
    /// <param name="bankAccount">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <param name="payAmount">amount of money for paying</param>
    /// <returns></returns>
    public ExceptionModel PayCredit(BankAccount? bankAccount, Credit credit, decimal payAmount)
    {
        if (bankAccount is null || credit is null)
            return ExceptionModel.VariableIsNull;

        var operationAccrualOnUserAccount = new Operation()
        {
            BankID = credit.BankID,
            ReceiverID = credit.UserBankAccountID,
            SenderID = credit.BankID,
            TransferAmount = payAmount
        };
        using var transaction = Database.BeginTransaction(IsolationLevel.Serializable);

        if (_bankContext.CreateOperation(operationAccrualOnUserAccount, OperationKind.Accrual) !=
            ExceptionModel.Successfully) // here creates operation for accrual money on user bank account
            return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

        // withdraw money to user's bank account
        if (_bankContext.BankAccountWithdraw(bankAccount,
                Banks.AsNoTracking().FirstOrDefault(x => x.ID == bankAccount.BankID),
                operationAccrualOnUserAccount) != ExceptionModel.Successfully)
            return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

        if (Delete(credit) != ExceptionModel.Successfully)
            return (ExceptionModel)operationAccrualOnUserAccount.OperationStatus.GetHashCode();

        transaction.Commit();

        return ExceptionModel.Successfully;
    }

    /// <summary>
    /// repays user's credit
    /// removes from the table field with credit's data of user
    /// </summary>
    /// <param name="bankAccount">from what account will withdraw money</param>
    /// <param name="credit">credit entity from database</param>
    /// <returns></returns>
    [Obsolete("This method is using for repaying a credit. " +
              "Instead of it you can use new method PayCredit that takes as third arg a value on which credit will decrease.")]
    public ExceptionModel RepayCredit(BankAccount? bankAccount, Credit? credit) =>
        PayCredit(bankAccount, credit, credit.CreditAmount);
}