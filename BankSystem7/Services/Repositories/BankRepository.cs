using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Models;

namespace BankSystem7.Services.Repositories;

public sealed class BankRepository<TUser> : IRepository<Bank> where TUser : User
{
    private BankContext<TUser> _bankContext;
    private ApplicationContext<TUser> _applicationContext;
    internal BankContext<TUser>? BankContext { get; set; }
    internal bool AnotherBankTransactionOperation { get; set; }

    private bool _disposedValue;
    public BankRepository()
    {
        _bankContext = BankServicesOptions<TUser>.BankContext ?? new BankContext<TUser>();
        BankContext = _bankContext;
        _applicationContext = BankServicesOptions<TUser>.ApplicationContext ??
                              new ApplicationContext<TUser>(BankServicesOptions<TUser>.Connection);
    }

    public BankRepository(string connection)
    {
        _bankContext = BankServicesOptions<TUser>.BankContext ?? new BankContext<TUser>(connection);
        BankContext = _bankContext;
        _applicationContext = BankServicesOptions<TUser>.ApplicationContext ??
                              new ApplicationContext<TUser>(connection);
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
            _bankContext.Dispose();
            BankContext.Dispose();
            _applicationContext.Dispose();
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _bankContext = null;
        BankContext = null;
        _applicationContext = null;
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

        var user = _applicationContext.Users.FirstOrDefault(x => x.ID == bankAccount.UserID);
        user.Card = _applicationContext.Cards.FirstOrDefault(x => x.UserID == user.ID);
            
        if (user is null)
            return ExceptionModel.VariableIsNull;

        bank.AccountAmount -= operation.TransferAmount;
        bankAccount.BankAccountAmount += operation.TransferAmount;
        user.Card.Amount = bankAccount.BankAccountAmount;
        _applicationContext.ChangeTracker.Clear();
        _applicationContext.BankAccounts.Update(bankAccount);
        _applicationContext.Banks.Update(bank);
        _applicationContext.Users.Update(user);
        _applicationContext.Cards.Update(user.Card);
        try
        {
            _applicationContext.SaveChanges();
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
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    /// <exception cref="Exception"></exception>
    public ExceptionModel BankAccountAccrual(User user, Operation operation)
    {
        if (user?.Card?.BankAccount?.Bank is null || !Exist(x => x.ID == user.Card.BankAccount.Bank.ID))
            return ExceptionModel.VariableIsNull;
        if (operation.OperationStatus != StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();
            
        if (user is null)
            return ExceptionModel.VariableIsNull;

        if (AnotherBankTransactionOperation)
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

            
        var user = _applicationContext.Users.FirstOrDefault(x => x.ID == bankAccount.UserID);
        user.Card = _applicationContext.Cards.FirstOrDefault(x => x.UserID == user.ID);
            
        if (user is null)
            return ExceptionModel.VariableIsNull;

        bank.AccountAmount += operation.TransferAmount;
        bankAccount.BankAccountAmount -= operation.TransferAmount;
        user.Card.Amount = bankAccount.BankAccountAmount;
        _applicationContext.ChangeTracker.Clear();
        _applicationContext.BankAccounts.Update(bankAccount); 
        _applicationContext.Banks.Update(bank);
        _applicationContext.Users.Update(user);
        _applicationContext.Cards.Update(user.Card);
        try
        {
            _applicationContext.SaveChanges();
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
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    /// <exception cref="Exception"></exception>
    public ExceptionModel BankAccountWithdraw(User user, Operation operation)
    {
        if (user?.Card?.BankAccount?.Bank is null || !Exist(x => x.ID == user.Card.BankAccount.Bank.ID))
            return ExceptionModel.VariableIsNull;
        if (operation.OperationStatus != StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        if (AnotherBankTransactionOperation)
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

        _applicationContext.ChangeTracker.Clear();
        if (Exist(x => x.ID == item.ID))
            Update(item);
        else
            _applicationContext.Add(item);
        
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(Bank item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.Remove(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public bool Exist(Expression<Func<Bank, bool>> predicate) => _applicationContext.Banks.AsNoTracking().Any(predicate);
    public bool FitsConditions(Bank? item)
    {
        return item is not null && Exist(x => x.ID == item.ID);
    }

    public IEnumerable<Bank> All => _applicationContext.Banks.AsNoTracking();

    public Bank? Get(Expression<Func<Bank, bool>> predicate) => _applicationContext.Banks.AsNoTracking().FirstOrDefault(predicate);

    public ExceptionModel Update(Bank item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.Banks.Update(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    ~BankRepository()
    {
        Dispose(false);
    }
}