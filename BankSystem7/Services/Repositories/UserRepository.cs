// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Data;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Repositories;


public sealed class UserRepository : IRepository<User>
{
    private BankAccountRepository _bankAccountRepository;
    private ApplicationContext _applicationContext;
    private BankRepository _bankRepository;
    private CardRepository _cardRepository;
    private bool _disposed;
    public UserRepository()
    {
        _bankAccountRepository = new BankAccountRepository(BankServicesOptions.Connection);
        _bankRepository = new BankRepository(BankServicesOptions.Connection);
        _cardRepository = new CardRepository(_bankAccountRepository);
        _applicationContext = BankServicesOptions.ApplicationContext ??
                              new ApplicationContext(_bankAccountRepository);
    }
    public UserRepository(string connection)
    {
        _bankAccountRepository = new BankAccountRepository(connection);
        _bankRepository = new BankRepository(connection);
        _cardRepository = new CardRepository(_bankAccountRepository);
        _applicationContext = BankServicesOptions.ApplicationContext ??
                              new ApplicationContext(_bankAccountRepository);
    }
    public UserRepository(BankAccountRepository repository)
    {
        _bankAccountRepository = repository;
        _bankRepository = BankServicesOptions.ServiceConfiguration?.BankRepository ?? new BankRepository(BankServicesOptions.Connection);
        _cardRepository = BankServicesOptions.ServiceConfiguration?.CardRepository ?? new CardRepository(_bankAccountRepository);
        _applicationContext = new ApplicationContext(_bankAccountRepository);
    }
    public ExceptionModel Create(User item)
    {
        if (item?.Card?.BankAccount?.Bank is null)
            return ExceptionModel.OperationFailed;
            
        //if user exists method will send false
        if (Exist(x => x.ID == item.ID))
            return ExceptionModel.OperationFailed;

        using var userCreationTransaction = _applicationContext.Database.BeginTransaction(IsolationLevel
                                                .RepeatableRead);
            
        
        var avoidDuplication = _applicationContext.AvoidDuplication(item.Card.BankAccount.Bank);
        if (avoidDuplication != ExceptionModel.Successfully)
        {
            userCreationTransaction.Rollback();
            return avoidDuplication;
        }
        
        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Users.Add(item);
        try
        {
            _applicationContext.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException.Message);
            userCreationTransaction.Rollback();
            throw;
        }

        userCreationTransaction.Commit();
        return ExceptionModel.Successfully;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    private void Dispose(bool disposing)
    {
        if (_disposed) return;
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

    public ExceptionModel Delete(User item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        
        _applicationContext.Users.Remove(item);
        try
        {
            _applicationContext.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        
        return _bankAccountRepository.Delete(item.Card.BankAccount);
    }

    public bool Exist(Expression<Func<User, bool>> predicate) => _applicationContext.Users.AsNoTracking().Any(predicate);
    public bool FitsConditions(User item)
    {
        return item?.Card?.BankAccount?.Bank is not null && Exist(x => x.ID == item.ID);
    }

    public IEnumerable<User> All => _applicationContext.Users.AsNoTracking();

    public User? Get(Expression<Func<User, bool>> predicate)
    {
        var user = _applicationContext.Users.AsNoTracking().FirstOrDefault(predicate);
        if (user is null)
            return user;
        user.Card = _cardRepository.Get(x => x.UserID == user.ID);
        if (user.Card is null)
            return user;
        user.Card.BankAccount = _bankAccountRepository.Get(x => x.UserID == user.ID);
        if (user.Card.BankAccount is null)
            return user;
        user.Card.BankAccount.Bank = _bankRepository.Get(x => x.BankAccounts.Contains(user.Card.BankAccount));
        if (user.Card.BankAccount.Bank is null)
            return user;
        return user;
    }

    public ExceptionModel Update(User item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        using var userUpdateTransaction = _applicationContext.Database.BeginTransaction(IsolationLevel.RepeatableRead);
            
        var bankAccountDeletionOperation = _bankAccountRepository.Update(item.Card.BankAccount);
        if (bankAccountDeletionOperation != ExceptionModel.Successfully)
        {
            userUpdateTransaction.Rollback();
            return bankAccountDeletionOperation;
        }
            
        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Users.Update(item);
        try
        {
            _applicationContext.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            userUpdateTransaction.Rollback();
            throw;
        }

        userUpdateTransaction.Commit();
        return ExceptionModel.Successfully;
    }

    ~UserRepository()
    {
        Dispose(false);
    }
}