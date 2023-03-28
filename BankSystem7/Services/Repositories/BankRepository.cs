using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Models;

namespace BankSystem7.Services.Repositories;

public sealed class BankRepository : IRepository<Bank>
{
    private BankAccountContext _bankAccountContext;
    private BankContext _bankContext;
    internal BankContext? BankContext { get; set; }

    private bool _disposedValue;
    public BankRepository()
    {
        _bankAccountContext = BankServicesOptions.BankAccountContext ?? new BankAccountContext();
        _bankContext = BankServicesOptions.BankContext ?? new BankContext();
        BankContext = _bankContext;
    }
    public BankRepository(string connection)
    {
        _bankAccountContext = BankServicesOptions.BankAccountContext ?? new BankAccountContext(connection);
        _bankContext = BankServicesOptions.BankContext ?? new BankContext(connection);
        BankContext = _bankContext;
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing)
        {
            if (BankContext is null) return;
            _bankAccountContext.Dispose();
            _bankContext.Dispose();
            BankContext.Dispose();
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _bankContext = null;
        _bankAccountContext = null;
        BankContext = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        _disposedValue = true;
    }

    /// <summary>
    /// asynchronously accrual money to user bank account from bank's account
    /// </summary>
    /// <param name="bankAccount"></param>
    /// <param name="bank"></param>
    /// <param name="operation"></param>
    /// <exception cref="Exception"></exception>
    public ExceptionModel BankAccountAccrual(BankAccount bankAccount, Bank bank, Operation operation)
    {
        if (bankAccount is null || bank is null || !Exist(x => x.ID == bank.ID))
            return ExceptionModel.VariableIsNull;
        if (operation.OperationStatus != StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        var user = _bankContext.Users.FirstOrDefault(x => x.ID == bankAccount.UserID);
        user.Card = _bankContext.Cards.FirstOrDefault(x => x.UserID == user.ID);
            
        if (user is null)
            return ExceptionModel.VariableIsNull;

        bank.AccountAmount -= operation.TransferAmount;
        bankAccount.BankAccountAmount += operation.TransferAmount;
        user.Card.Amount = bankAccount.BankAccountAmount;
        _bankContext.ChangeTracker.Clear();
        _bankAccountContext.ChangeTracker.Clear();
        _bankContext.BankAccounts.Update(bankAccount);
        _bankContext.Banks.Update(bank);
        _bankContext.Users.Update(user);
        _bankContext.Cards.Update(user.Card);
        try
        {
            _bankContext.SaveChanges();
            _bankContext.DeleteOperation(operation);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return ExceptionModel.Successfully;
    }
        
    /// <summary>
    /// asynchronously accrual money to user bank account from bank's account
    /// </summary>
    /// <param name="user"></param>
    /// <param name="bank"></param>
    /// <param name="operation"></param>
    /// <exception cref="Exception"></exception>
    public ExceptionModel BankAccountAccrual(User user, Bank bank, Operation operation)
    {
        if (user is null || operation is null || user.Card is null || user.Card.BankAccount is null ||
            bank is null || !Exist(x => x.ID == bank.ID))
            return ExceptionModel.VariableIsNull;
        if (operation.OperationStatus != StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();
            
        if (user is null)
            return ExceptionModel.VariableIsNull;

        bank.AccountAmount -= operation.TransferAmount;
        user.Card.BankAccount.BankAccountAmount += operation.TransferAmount;
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;
        _bankContext.ChangeTracker.Clear();
        _bankContext.Users.Update(user);
        _bankContext.Banks.Update(bank);
        try
        {
            _bankContext.SaveChanges();
            _bankContext.DeleteOperation(operation);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return ExceptionModel.Successfully;
    }

    /// <summary>
    /// asynchronously withdraw money from user bank account and accrual to bank's account
    /// </summary>
    /// <param name="bankAccount"></param>
    /// <param name="bank"></param>
    /// <param name="operation"></param>
    /// <exception cref="Exception"></exception>
    public ExceptionModel BankAccountWithdraw(BankAccount bankAccount, Bank bank, Operation operation)
    {
        if (bankAccount is null || bank is null)
            return ExceptionModel.VariableIsNull;
        if (operation.OperationStatus != StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

            
        var user = _bankContext.Users.FirstOrDefault(x => x.ID == bankAccount.UserID);
        user.Card = _bankContext.Cards.FirstOrDefault(x => x.UserID == user.ID);
            
        if (user is null)
            return ExceptionModel.VariableIsNull;

        bank.AccountAmount += operation.TransferAmount;
        bankAccount.BankAccountAmount -= operation.TransferAmount;
        user.Card.Amount = bankAccount.BankAccountAmount;
        _bankContext.ChangeTracker.Clear();
        _bankAccountContext.ChangeTracker.Clear();
        _bankContext.BankAccounts.Update(bankAccount);
        _bankContext.Banks.Update(bank);
        _bankContext.Users.Update(user);
        _bankContext.Cards.Update(user.Card);
        try
        {
            _bankContext.SaveChanges();
            _bankContext.DeleteOperation(operation); // doesn't exist operation
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return ExceptionModel.Successfully;
    }
    
    /// <summary>
    /// asynchronously withdraw money from user bank account and accrual to bank's account
    /// </summary>
    /// <param name="user"></param>
    /// <param name="bank"></param>
    /// <param name="operation"></param>
    /// <exception cref="Exception"></exception>
    public ExceptionModel BankAccountWithdraw(User user, Bank bank, Operation operation)
    {
        if (user is null || bank is null)
            return ExceptionModel.VariableIsNull;
        if (operation.OperationStatus != StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        bank.AccountAmount += operation.TransferAmount;
        user.Card.BankAccount.BankAccountAmount -= operation.TransferAmount;
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;
        _bankContext.ChangeTracker.Clear();
        _bankContext.Users.Update(user);
        _bankContext.Banks.Update(bank);
        try
        {
            _bankContext.SaveChanges();
            _bankContext.DeleteOperation(operation); // doesn't exist operation
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return ExceptionModel.Successfully;
    }
    public ExceptionModel Create(Bank item)
    {
        if (item is null)
            return ExceptionModel.VariableIsNull;

        if (Exist(x => x.ID == item.ID))
            _bankContext.Update(item);
        else
            _bankContext.Add(item);
        
        _bankContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(Bank item)
    {
        if (item is null || !Exist(x => x.ID == item.ID))
            return ExceptionModel.OperationFailed;

        _bankContext.Remove(item);
        _bankContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public bool Exist(Expression<Func<Bank, bool>> predicate) => _bankContext.Banks.AsNoTracking().Any(predicate);

    public IEnumerable<Bank> All => _bankContext.Banks.AsNoTracking();

    public Bank? Get(Expression<Func<Bank, bool>> predicate) => _bankContext.Banks.AsNoTracking().FirstOrDefault(predicate);

    /// <summary>
    /// repays user's credit
    /// removes from the table field with credit's data of user
    /// </summary>
    /// <param name="bankAccount"></param>
    /// <param name="credit"></param>
    /// <returns></returns>
    public ExceptionModel RepayCredit(BankAccount bankAccount, Credit credit) => _bankContext.RepayCredit(bankAccount, credit);

    /// <summary>
    /// repays user's credit
    /// removes from the table field with credit's data of user
    /// </summary>
    /// <param name="bankAccount"></param>
    /// <param name="credit"></param>
    /// <returns></returns>
    public ExceptionModel TakeCredit(BankAccount bankAccount, Credit credit) => _bankContext.TakeCredit(bankAccount, credit);

    internal ExceptionModel AvoidDuplication(Bank item)
    {
        foreach (var bankAccount in item.BankAccounts)
            _bankContext.Entry(bankAccount).State = EntityState.Unchanged;

        foreach (var credit in item.Credits)
            _bankContext.Entry(credit).State = EntityState.Unchanged;

        return ExceptionModel.Successfully;
    }

    public ExceptionModel Update(Bank item)
    {
        if (item is null)
            return ExceptionModel.VariableIsNull;

        if (!Exist(x => x.ID == item.ID))
            return ExceptionModel.OperationFailed;

        _bankContext.Banks.Update(item);
        _bankContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    ~BankRepository()
    {
        Dispose(false);
    }
}