using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Models;

namespace BankSystem7.Services.Repositories;

public sealed class BankRepository : ApplicationContext, IRepository<Bank>
{
    private BankContext _bankContext;
    internal BankContext? BankContext { get; set; }

    private bool _disposedValue;
    public BankRepository()
    {
        _bankContext = BankServicesOptions.BankContext ?? new BankContext();
        BankContext = _bankContext;
    }

    public BankRepository(string connection) : base(connection)
    {
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
        if (_disposedValue) 
            return;
        if (disposing)
        {
            if (BankContext is null) 
                return;
            Dispose();
            BankContext.Dispose();
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _bankContext = null;
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
    
    [Obsolete("Don't use this method. Instead use the same method but with another parameters: BankAccountAccrual(User user, Operation operation)")]
    public ExceptionModel BankAccountAccrual(BankAccount bankAccount, Bank bank, Operation operation)
    {
        if (bankAccount is null || bank is null || !Exist(x => x.ID == bank.ID))
            return ExceptionModel.VariableIsNull;
        if (operation.OperationStatus != StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        var user = Users.FirstOrDefault(x => x.ID == bankAccount.UserID);
        user.Card = Cards.FirstOrDefault(x => x.UserID == user.ID);
            
        if (user is null)
            return ExceptionModel.VariableIsNull;

        bank.AccountAmount -= operation.TransferAmount;
        bankAccount.BankAccountAmount += operation.TransferAmount;
        user.Card.Amount = bankAccount.BankAccountAmount;
        ChangeTracker.Clear();
        BankAccounts.Update(bankAccount);
        Banks.Update(bank);
        Users.Update(user);
        Cards.Update(user.Card);
        try
        {
            SaveChanges();
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
    public ExceptionModel BankAccountAccrual(User user, Operation operation)
    {
        if (user?.Card?.BankAccount?.Bank is null || !Exist(x => x.ID == user.Card.BankAccount.Bank.ID))
            return ExceptionModel.VariableIsNull;
        if (operation.OperationStatus != StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();
            
        if (user is null)
            return ExceptionModel.VariableIsNull;

        user.Card.BankAccount.Bank.AccountAmount -= operation.TransferAmount;
        user.Card.BankAccount.BankAccountAmount += operation.TransferAmount;
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;
        _bankContext.ChangeTracker.Clear();
        _bankContext.Update(user);
        try
        {
            _bankContext.SaveChanges();
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
    [Obsolete("Don't use this method. Instead use the same method but with another parameters: BankAccountWithdraw(User user, Operation operation)")]
    public ExceptionModel BankAccountWithdraw(BankAccount bankAccount, Bank bank, Operation operation)
    {
        if (bankAccount is null || bank is null)
            return ExceptionModel.VariableIsNull;
        if (operation.OperationStatus != StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

            
        var user = Users.FirstOrDefault(x => x.ID == bankAccount.UserID);
        user.Card = Cards.FirstOrDefault(x => x.UserID == user.ID);
            
        if (user is null)
            return ExceptionModel.VariableIsNull;

        bank.AccountAmount += operation.TransferAmount;
        bankAccount.BankAccountAmount -= operation.TransferAmount;
        user.Card.Amount = bankAccount.BankAccountAmount;
        ChangeTracker.Clear();
        BankAccounts.Update(bankAccount); 
        Banks.Update(bank);
        Users.Update(user);
        Cards.Update(user.Card);
        try
        {
            SaveChanges();
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
    public ExceptionModel BankAccountWithdraw(User user, Operation operation)
    {
        if (user?.Card?.BankAccount?.Bank is null || !Exist(x => x.ID == user.Card.BankAccount.Bank.ID))
            return ExceptionModel.VariableIsNull;
        if (operation.OperationStatus != StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        user.Card.BankAccount.Bank.AccountAmount += operation.TransferAmount;
        user.Card.BankAccount.BankAccountAmount -= operation.TransferAmount;
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;
        _bankContext.ChangeTracker.Clear();
        _bankContext.Update(user);
        try
        {
            _bankContext.SaveChanges();
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
            Update(item);
        else
            Add(item);
        
        SaveChanges();
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(Bank item)
    {
        if (item is null || !Exist(x => x.ID == item.ID))
            return ExceptionModel.OperationFailed;

        Remove(item);
        SaveChanges();
        return ExceptionModel.Successfully;
    }

    public bool Exist(Expression<Func<Bank, bool>> predicate) => Banks.AsNoTracking().Any(predicate);

    public IEnumerable<Bank> All => Banks.AsNoTracking();

    public Bank? Get(Expression<Func<Bank, bool>> predicate) => Banks.AsNoTracking().FirstOrDefault(predicate);

    /// <summary>
    /// repays user's credit
    /// removes from the table field with credit's data of user
    /// </summary>
    /// <param name="bankAccount"></param>
    /// <param name="credit"></param>
    /// <returns></returns>
    public ExceptionModel RepayCredit(BankAccount bankAccount, Credit credit) => 
        _bankContext.RepayCredit(bankAccount, credit);

    /// <summary>
    /// repays user's credit
    /// removes from the table field with credit's data of user
    /// </summary>
    /// <param name="bankAccount"></param>
    /// <param name="credit"></param>
    /// <returns></returns>
    public ExceptionModel TakeCredit(BankAccount bankAccount, Credit credit) =>
        _bankContext.TakeCredit(bankAccount, credit);

    internal ExceptionModel AvoidDuplication(Bank item)
    {
        foreach (var bankAccount in item.BankAccounts)
            Entry(bankAccount).State = EntityState.Unchanged;

        foreach (var credit in item.Credits)
            Entry(credit).State = EntityState.Unchanged;

        return ExceptionModel.Successfully;
    }

    public ExceptionModel Update(Bank item)
    {
        if (item is null)
            return ExceptionModel.VariableIsNull;

        if (!Exist(x => x.ID == item.ID))
            return ExceptionModel.OperationFailed;

        Banks.Update(item);
        SaveChanges();
        return ExceptionModel.Successfully;
    }

    ~BankRepository()
    {
        Dispose(false);
    }
}