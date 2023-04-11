﻿using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;

namespace BankSystem7.Services.Repositories;

public sealed class CardRepository<TUser, TCard, TBankAccount, TBank, TCredit> : IRepository<TCard>
    where TUser : User 
    where TCard : Card 
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit> _bankAccountRepository;
    private ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> _applicationContext;
    private bool _disposedValue;

    public CardRepository()
    {
        _bankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
    }
    public CardRepository(BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit> bankAccountRepository)
    {
        _bankAccountRepository = bankAccountRepository;
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(bankAccountRepository);
    }
    public CardRepository(string connection)
    {
        _bankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
    }

    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Private implementation of Dispose pattern.
    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing)
        {
            _bankAccountRepository.Dispose();
            _applicationContext.Dispose();
        }
        _bankAccountRepository = null;
        _applicationContext = null;
        _disposedValue = true;
    }

    public ExceptionModel Update(TCard item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        _applicationContext.Cards.Update(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    /// <summary>
    /// use this method only for creating new card. 
    /// if you'll try to use this method in designing card you'll get big bug which 'll force you program work wrong.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public ExceptionModel Create(TCard item)
    {
        if (item.Exception == Warning.AgeRestricted)
            return ExceptionModel.OperationRestricted;
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        _applicationContext.Cards.Add(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(TCard item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        _applicationContext.Cards.Remove(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public bool Exist(Expression<Func<TCard, bool>> predicate) => _applicationContext.Cards.AsNoTracking().Any(predicate);
    public bool FitsConditions(TCard? item)
    {
        return item is not null && Exist(x => x.ID == item.ID);
    }

    public TCard? Get(Expression<Func<TCard, bool>> predicate) => _applicationContext.Cards.AsNoTracking().FirstOrDefault(predicate);

    public IEnumerable<TCard> All => _applicationContext.Cards.AsNoTracking();
    public void ChangeTrackerCardContext() => _applicationContext.ChangeTracker.Clear();

    ~CardRepository()
    {
        Dispose(false);
    }
}