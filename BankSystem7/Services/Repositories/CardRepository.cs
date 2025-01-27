﻿using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BankSystem7.Services.Repositories;

public sealed class CardRepository<TUser, TCard, TBankAccount, TBank, TCredit> : ICardRepository<TCard>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private readonly IBankAccountRepository<TUser, TBankAccount> _bankAccountRepository;
    private readonly ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> _applicationContext;
    private bool _disposedValue;

    public CardRepository()
    {
        _bankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
    }

    public CardRepository(IBankAccountRepository<TUser, TBankAccount> bankAccountRepository)
    {
        _bankAccountRepository = bankAccountRepository;
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>
                              (ServicesSettings.Connection);
    }

    public CardRepository(string connection)
    {
        _bankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
    }

    public IQueryable<TCard> All =>
        _applicationContext.Cards
            .Include(x => x.User)
            .Include(x => x.BankAccount)
            .ThenInclude(x => x.Bank)
            .AsNoTracking() ?? Enumerable.Empty<TCard>().AsQueryable();

    public IQueryable<TCard> AllWithTracking =>
        _applicationContext.Cards
            .Include(x => x.User)
            .Include(x => x.BankAccount)
            .ThenInclude(x => x.Bank) ?? Enumerable.Empty<TCard>().AsQueryable();

    /// <summary>
    /// use this method only for creating new card.
    /// if you'll try to use this method in designing card you'll get big bug which 'll force you program work wrong.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public ExceptionModel Create(TCard item)
    {
        if (item.Exception == CardException.AgeRestricted)
            return ExceptionModel.OperationRestricted;

        if (item is not null && Exist(x => x.ID == item.ID))
            return ExceptionModel.OperationFailed;

        _applicationContext.Cards.Add(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> CreateAsync(TCard item)
    {
        if (item.Exception == CardException.AgeRestricted)
            return ExceptionModel.OperationRestricted;

        if (item is not null && Exist(x => x.ID == item.ID))
            return ExceptionModel.OperationFailed;

        _applicationContext.Cards.Add(item);
        await _applicationContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    public ExceptionModel Delete(TCard item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.Cards.Remove(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> DeleteAsync(TCard item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.Cards.Remove(item);
        await _applicationContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    public ExceptionModel Update(TCard item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.Cards.Update(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> UpdateAsync(TCard item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.Cards.Update(item);
        await _applicationContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    public TCard Get(Expression<Func<TCard, bool>> predicate)
    {
        return All.FirstOrDefault(predicate) ?? (TCard)Card.Default;
    }

    public async Task<TCard> GetAsync(Expression<Func<TCard, bool>> predicate)
    {
        return await All.FirstOrDefaultAsync(predicate) ?? (TCard)Card.Default;
    }

    public TCard GetWithTracking(Expression<Func<TCard, bool>> predicate)
    {
        return AllWithTracking.FirstOrDefault(predicate) ?? (TCard)Card.Default;
    }

    public bool Exist(Expression<Func<TCard, bool>> predicate)
    {
        return All.Any(predicate);
    }

    public async Task<bool> ExistAsync(Expression<Func<TCard, bool>> predicate)
    {
        return await All.AnyAsync(predicate);
    }

    public bool ExistWithTracking(Expression<Func<TCard, bool>> predicate)
    {
        return AllWithTracking.Any(predicate);
    }

    public bool FitsConditions(TCard? item)
    {
        return item is not null && Exist(x => x.ID == item.ID);
    }

    public async Task<bool> FitsConditionsAsync(TCard? item)
    {
        return item is not null && await ExistAsync(x => x.ID == item.ID);
    }

    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose()
    {
        if (_disposedValue)
            return;

        _bankAccountRepository?.Dispose();
        _applicationContext?.Dispose();

        _disposedValue = true;
    }
}