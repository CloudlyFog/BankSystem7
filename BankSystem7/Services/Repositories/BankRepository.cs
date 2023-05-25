﻿using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BankSystem7.Services.Repositories;

public sealed class BankRepository<TUser, TCard, TBankAccount, TBank, TCredit> : IRepository<TBank>, 
    IReaderServiceWithTracking<TBank>, IRepositoryAsync<TBank>
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
    /// <exception cref="Exception"></exception>
    public ExceptionModel BankAccountAccrual(TUser user, Operation operation)
    {
        if (user?.Card?.BankAccount?.Bank is null || !Exist(x => x.ID == user.Card.BankAccount.Bank.ID))
            return ExceptionModel.EntityIsNull;

        if (operation.OperationStatus != StatusOperationCode.Ok)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        if (user is null)
            return ExceptionModel.EntityIsNull;

        if (AnotherBankTransactionOperation)
            user.Card.BankAccount.Bank.AccountAmount -= operation.TransferAmount;

        user.Card.BankAccount.BankAccountAmount += operation.TransferAmount;
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;

        _bankContext.Update(user);
        _bankContext.SaveChanges();
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
        if (operation.OperationStatus != StatusOperationCode.Ok)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        if (AnotherBankTransactionOperation)
            user.Card.BankAccount.Bank.AccountAmount += operation.TransferAmount;

        user.Card.BankAccount.BankAccountAmount -= operation.TransferAmount;
        user.Card.Amount = user.Card.BankAccount.BankAccountAmount;
        _bankContext.ChangeTracker.Clear();
        _bankContext.Update(user);
        _bankContext.SaveChanges();
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

    internal decimal CalculateBankAccountAmount(decimal oldValue, decimal newValue)
    {
        return newValue - oldValue;
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