using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Repositories;

public sealed class BankRepository<TUser, TCard, TBankAccount, TBank, TCredit> : IRepository<TBank>, IReaderServiceWithTracking<TBank>
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
            return ExceptionModel.EntityIsNull;
        if (operation.OperationStatus != StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        if (user is null)
            return ExceptionModel.EntityIsNull;

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
        return ExceptionModel.Ok;
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
            return ExceptionModel.EntityIsNull;
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
        return ExceptionModel.Ok;
    }

    public ExceptionModel Create(TBank item)
    {
        if (item is null)
            return ExceptionModel.EntityIsNull;

        _applicationContext.ChangeTracker.Clear();
        if (Exist(x => x.ID == item.ID))
            return Update(item);

        _applicationContext.Add(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public ExceptionModel Delete(TBank item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Remove(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public bool Exist(Func<TBank, bool> predicate)
    {
        return All.Any(predicate);
    }

    public bool FitsConditions(TBank? item)
    {
        return item is not null && Exist(x => x.ID == item.ID);
    }

    public IEnumerable<TBank> All =>
        _applicationContext.Banks
        .Include(x => x.BankAccounts)
        .Include(x => x.Credits)
        .AsNoTracking() ?? Enumerable.Empty<TBank>();

    public IEnumerable<TBank> AllWithTracking => 
        _applicationContext.Banks
        .Include(x => x.BankAccounts)
        .Include(x => x.Credits) ?? Enumerable.Empty<TBank>();


    public TBank Get(Func<TBank, bool> predicate)
    {
        return _applicationContext.Banks
        .Include(x => x.BankAccounts)
        .Include(x => x.Credits)
        .AsNoTracking().AsEnumerable()
        .FirstOrDefault(predicate) ?? (TBank)Bank.Default;
    }

    public ExceptionModel Update(TBank item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Banks.Update(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    internal decimal CalculateBankAccountAmount(decimal oldValue, decimal newValue)
    {
        return newValue - oldValue;
    }

    public TBank GetWithTracking(Func<TBank, bool> predicate)
    {
        return AllWithTracking.FirstOrDefault(predicate) ?? (TBank)Bank.Default;
    }

    public bool ExistWithTracking(Func<TBank, bool> predicate)
    {
        return AllWithTracking.Any(predicate);
    }

    ~BankRepository()
    {
        Dispose(false);
    }
}