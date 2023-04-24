﻿using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BankSystem7.Services.Repositories;

public sealed class UserRepository<TUser, TCard, TBankAccount, TBank, TCredit> : LoggerExecutor<OperationType>, IRepository<TUser>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit> _bankAccountRepository;
    private ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> _applicationContext;
    private BankRepository<TUser, TCard, TBankAccount, TBank, TCredit> _bankRepository;
    private CardRepository<TUser, TCard, TBankAccount, TBank, TCredit> _cardRepository;
    private bool _disposed;

    public UserRepository()
    {
        _bankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
        _bankRepository = new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
        _cardRepository = new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>
                              (BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
    }

    public UserRepository(string connection)
    {
        _bankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        _bankRepository = new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        _cardRepository = new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>
                              (BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
    }

    public UserRepository(BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit> repository)
    {
        _bankAccountRepository = repository;
        _bankRepository = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration?.BankRepository ?? new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
        _cardRepository = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration?.CardRepository ?? new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _applicationContext = new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>
            (BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing)
        {
            _bankAccountRepository.Dispose();
            _bankRepository.Dispose();
            _cardRepository.Dispose();
            _applicationContext.Dispose();
        }
        _bankAccountRepository = null;
        _bankRepository = null;
        _cardRepository = null;
        _applicationContext = null;
        _disposed = true;
    }

    public ExceptionModel Create(TUser item)
    {
        if (item?.Card?.BankAccount?.Bank is null)
            return ExceptionModel.OperationFailed;

        //if user exists method will send false
        if (Exist(x => x.ID.Equals(item.ID) || x.Name.Equals(item.Name) && x.Email.Equals(item.Email)))
            return ExceptionModel.OperationRestricted;
        using var userCreationTransaction = _applicationContext.Database.BeginTransaction(IsolationLevel
                                                .RepeatableRead);

        var avoidDuplication = _applicationContext.AvoidDuplication(item.Card.BankAccount.Bank);
        if (avoidDuplication != ExceptionModel.Successfully)
        {
            userCreationTransaction.Rollback();
            return avoidDuplication;
        }

        Bank bank = null;

        if (_bankRepository.Exist(x => x.ID == item.Card.BankAccount.Bank.ID || x.BankName == item.Card.BankAccount.Bank.BankName))
        {
            bank = item.Card.BankAccount.Bank;
            item.Card.BankAccount.Bank = null;
        }

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Users.Add(item);

        try
        {
            _applicationContext.SaveChanges();
        }
        catch (Exception)
        {
            userCreationTransaction.Rollback();
            return ExceptionModel.ThrewException;
        }

        if (bank is null)
        {
            userCreationTransaction.Commit();
            return ExceptionModel.Successfully;
        }
        item.Card.BankAccount.Bank ??= bank;

        var updateBank = _bankRepository.Update(_bankRepository.Get(x => x.ID == item.Card.BankAccount.Bank.ID));
        if (updateBank != ExceptionModel.Successfully)
        {
            userCreationTransaction.Rollback();
            return updateBank;
        }

        userCreationTransaction.Commit();

        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationNotExist;

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Users.Remove(item);
        try
        {
            _applicationContext.SaveChanges();
        }
        catch (Exception)
        {
            return ExceptionModel.ThrewException;
        }
        return _bankAccountRepository.Delete(_bankAccountRepository.Get(x => x.ID == item.Card.BankAccount.ID));
    }

    public bool Exist(Func<TUser, bool> predicate) =>
        _applicationContext.Users
            .Include(x => x.Card)
            .ThenInclude(x => x.BankAccount)
            .ThenInclude(x => x.Bank)
            .AsNoTracking().Any(predicate);

    public bool FitsConditions(TUser? item)
    {
        if (item?.Card?.BankAccount?.Bank is null)
            return false;

        if (!Exist(x => x.ID == item.ID))
            return false;

        return true;
    }

    public IEnumerable<TUser> All =>
        _applicationContext.Users
            .Include(x => x.Card)
            .ThenInclude(x => x.BankAccount)
            .ThenInclude(x => x.Bank)
            .AsNoTracking() ?? Enumerable.Empty<TUser>();

    public TUser Get(Func<TUser, bool> predicate)
    {
        return _applicationContext.Users
            .Include(x => x.Card)
            .ThenInclude(x => x.BankAccount)
            .ThenInclude(x => x.Bank)
            .AsNoTracking().AsEnumerable()
            .FirstOrDefault(predicate) ?? (TUser)User.Default;
    }

    public ExceptionModel Update(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        using var userUpdateTransaction = _applicationContext.Database.BeginTransaction(IsolationLevel.RepeatableRead);

        var bankAccountUpdateOperation = _bankAccountRepository
            .Update(_bankAccountRepository.Get(x => x.ID == item.Card.BankAccount.ID));
        if (bankAccountUpdateOperation != ExceptionModel.Successfully)
        {
            userUpdateTransaction.Rollback();
            return bankAccountUpdateOperation;
        }

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Users.Update(item);
        try
        {
            _applicationContext.SaveChanges();
        }
        catch (Exception)
        {
            userUpdateTransaction.Rollback();
            return ExceptionModel.ThrewException;
        }

        userUpdateTransaction.Commit();
        return ExceptionModel.Successfully;
    }

    ~UserRepository()
    {
        Dispose(false);
    }
}