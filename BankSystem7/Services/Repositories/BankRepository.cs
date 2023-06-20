using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Interfaces;
using BankSystem7.Services.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BankSystem7.Services.Repositories;

public sealed class BankRepository<TUser, TCard, TBankAccount, TBank, TCredit> : IBankRepository<TUser, TBank>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private readonly BankContext<TUser, TCard, TBankAccount, TBank, TCredit> _bankContext;
    private readonly ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> _applicationContext;
    internal BankContext<TUser, TCard, TBankAccount, TBank, TCredit>? BankContext { get; set; }
    internal bool AnotherBankTransactionOperation { get; set; }

    private bool _disposedValue;

    public BankRepository()
    {
        _bankContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.BankContext ?? new BankContext<TUser, TCard, TBankAccount, TBank, TCredit>();
        BankContext = _bankContext;
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>
                                  (ServicesSettings.Connection);
    }

    public BankRepository(string connection)
    {
        _bankContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.BankContext ?? new BankContext<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        BankContext = _bankContext;
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
    }

    public IQueryable<TBank> All =>
        _applicationContext.Banks
            .Include(x => x.BankAccounts)
            .Include(x => x.Credits)
            .AsNoTracking();

    public IQueryable<TBank> AllWithTracking =>
        _applicationContext.Banks
            .Include(x => x.BankAccounts)
            .Include(x => x.Credits);

    /// <summary>
    /// accrual money to user bank account from bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public ExceptionModel BankAccountAccrual(TUser user, Operation operation)
    {
        // Check if the operation status is OK.
        if (operation.OperationStatus is not StatusOperationCode.Ok or StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        // If the transaction is with another bank, subtract the transfer amount from the user's bank account.
        if (AnotherBankTransactionOperation)
            user.Card.BankAccount.Bank.AccountAmount -= operation.TransferAmount;

        // Add the transfer amount to the user's bank account.
        user.Card.BankAccount.BankAccountAmount += operation.TransferAmount;

        // Update the user's card amount to reflect the new bank account amount.
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;

        // Update the user's information in the database.
        _bankContext.Update(user);
        _bankContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// accrual money to user bank account from bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public async Task<ExceptionModel> BankAccountAccrualAsync(TUser user, Operation operation)
    {
        // Check if the operation status is OK.
        if (operation.OperationStatus is not StatusOperationCode.Ok or StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        // If the transaction is with another bank, subtract the transfer amount from the user's bank account.
        if (AnotherBankTransactionOperation)
            user.Card.BankAccount.Bank.AccountAmount -= operation.TransferAmount;

        // Add the transfer amount to the user's bank account.
        user.Card.BankAccount.BankAccountAmount += operation.TransferAmount;

        // Update the user's card amount to reflect the new bank account amount.
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;

        // Update the user's information in the database.
        _bankContext.Update(user);
        await _bankContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// withdraw money from user bank account and accrual to bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public ExceptionModel BankAccountWithdraw(TUser user, Operation operation)
    {
        // Check if the operation status is OK.
        if (operation.OperationStatus is not StatusOperationCode.Ok or StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        // Check if the user's card's bank account's bank is null or if the bank ID does not exist in the database
        if (user?.Card?.BankAccount?.Bank is null || !Exist(x => x.ID == user.Card.BankAccount.Bank.ID))
            return ExceptionModel.EntityIsNull;

        // If it is another bank transaction operation, add the transfer amount to the bank account amount of the user's card's bank
        if (AnotherBankTransactionOperation)
            user.Card.BankAccount.Bank.AccountAmount += operation.TransferAmount;

        // Subtract the transfer amount from the bank account amount of the user's card's bank and update the user's card amount
        user.Card.BankAccount.BankAccountAmount -= operation.TransferAmount;
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;

        // Update the user's information in the database and save changes
        _bankContext.Update(user);
        _bankContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// withdraw money from user bank account and accrual to bank's account
    /// </summary>
    /// <param name="user">where will accrued money</param>
    /// <param name="operation">data of ongoing operation</param>
    public async Task<ExceptionModel> BankAccountWithdrawAsync(TUser user, Operation operation)
    {
        // Check if the operation status is OK.
        if (operation.OperationStatus is not StatusOperationCode.Ok or StatusOperationCode.Successfully)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        // Check if the user's card's bank account's bank is null or if the bank ID does not exist in the database
        if (user?.Card?.BankAccount?.Bank is null || !Exist(x => x.ID == user.Card.BankAccount.Bank.ID))
            return ExceptionModel.EntityIsNull;

        // If it is another bank transaction operation, add the transfer amount to the bank account amount of the user's card's bank
        if (AnotherBankTransactionOperation)
            user.Card.BankAccount.Bank.AccountAmount += operation.TransferAmount;

        // Subtract the transfer amount from the bank account amount of the user's card's bank and update the user's card amount
        user.Card.BankAccount.BankAccountAmount -= operation.TransferAmount;
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;

        // Update the user's information in the database and save changes
        _bankContext.Update(user);
        await _bankContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    public ExceptionModel Create(TBank item)
    {
        if (item is null)
            return ExceptionModel.EntityIsNull;

        if (Exist(x => x.ID == item.ID))
            return Update(item);

        _applicationContext.Add(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> CreateAsync(TBank item)
    {
        if (item is null)
            return ExceptionModel.EntityIsNull;

        if (Exist(x => x.ID == item.ID))
            return Update(item);

        _applicationContext.Add(item);
        await _applicationContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    public ExceptionModel Delete(TBank item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.Remove(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> DeleteAsync(TBank item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.Remove(item);
        await _applicationContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    public ExceptionModel Update(TBank item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.Banks.Update(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> UpdateAsync(TBank item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.Banks.Update(item);
        await _applicationContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    public TBank Get(Expression<Func<TBank, bool>> predicate)
    {
        return All.FirstOrDefault(predicate) ?? (TBank)Bank.Default;
    }

    public async Task<TBank> GetAsync(Expression<Func<TBank, bool>> predicate)
    {
        return await All.FirstOrDefaultAsync(predicate) ?? (TBank)Bank.Default;
    }

    public TBank GetWithTracking(Expression<Func<TBank, bool>> predicate)
    {
        return AllWithTracking.FirstOrDefault(predicate) ?? (TBank)Bank.Default;
    }

    public bool Exist(Expression<Func<TBank, bool>> predicate)
    {
        return All.Any(predicate);
    }

    public async Task<bool> ExistAsync(Expression<Func<TBank, bool>> predicate)
    {
        return await All.AnyAsync(predicate);
    }

    public bool ExistWithTracking(Expression<Func<TBank, bool>> predicate)
    {
        return AllWithTracking.Any(predicate);
    }

    public bool FitsConditions(TBank? item)
    {
        return item is not null && Exist(x => x.ID == item.ID);
    }

    public async Task<bool> FitsConditionsAsync(TBank? item)
    {
        return item is not null && await ExistAsync(x => x.ID == item.ID);
    }

    /// <summary>
    /// Calculates the bank account amount.
    /// </summary>
    /// <param name="accountAmountValue">The account amount value.</param>
    /// <returns></returns>
    public decimal CalculateBankAccountAmount(decimal accountAmountValue)
    {
        return accountAmountValue - 0;
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
        BankContext = null;
        _disposedValue = true;
    }

    ~BankRepository()
    {
        Dispose(false);
    }
}