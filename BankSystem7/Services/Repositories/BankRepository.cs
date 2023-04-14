using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BankSystem7.Services.Repositories;

public sealed class BankRepository<TUser, TCard, TBankAccount, TBank, TCredit> : IRepository<TBank>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private BankContext<TUser, TCard, TBankAccount, TBank, TCredit> _bankContext;
    private ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> _applicationContext;
    internal BankContext<TUser, TCard, TBankAccount, TBank, TCredit>? BankContext { get; set; }
    internal bool AnotherBankTransactionOperation { get; set; }

    private bool _disposedValue;

    public BankRepository()
    {
        _bankContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.BankContext ?? new BankContext<TUser, TCard, TBankAccount, TBank, TCredit>();
        BankContext = _bankContext;
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>
                                  (BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
    }

    public BankRepository(string connection)
    {
        _bankContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.BankContext ?? new BankContext<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        BankContext = _bankContext;
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
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
    /// accrual money to user bank account from bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    /// <exception cref="Exception"></exception>
    public ExceptionModel BankAccountAccrual(TUser user, Operation operation)
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
    /// withdraw money from user bank account and accrual to bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    /// <exception cref="Exception"></exception>
    public ExceptionModel BankAccountWithdraw(TUser user, Operation operation)
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

    public ExceptionModel Create(TBank item)
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

    public ExceptionModel Delete(TBank item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Remove(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public bool Exist(Expression<Func<TBank, bool>> predicate) => _applicationContext.Banks.AsNoTracking().Any(predicate);

    public bool FitsConditions(TBank? item)
    {
        return item is not null && Exist(x => x.ID == item.ID);
    }

    public IEnumerable<TBank> All => _applicationContext.Banks
        .Include(x => x.BankAccounts).Include(x => x.Credits).AsNoTracking();

    public TBank? Get(Expression<Func<TBank, bool>> predicate) => _applicationContext.Banks
        .Include(x => x.BankAccounts).Include(x => x.Credits)
        .AsNoTracking().FirstOrDefault(predicate);

    public ExceptionModel Update(TBank item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Banks.Update(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    ~BankRepository()
    {
        Dispose(false);
    }
}