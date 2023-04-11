using System.Data;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
    private ILogger _logger;
    private List<GeneralReport<OperationType>> _reports = new();
    private bool _disposed;
    public UserRepository()
    {
        _bankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
        _bankRepository = new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
        _cardRepository = new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _logger = new Logger<TUser, TCard, TBankAccount, TBank, TCredit>(ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>.Options.LoggerOptions);
    }
    public UserRepository(string connection)
    {
        _bankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        _bankRepository = new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        _cardRepository = new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _logger = new Logger<TUser, TCard, TBankAccount, TBank, TCredit>(ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>.Options.LoggerOptions);
        
    }
    public UserRepository(BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit> repository)
    {
        _bankAccountRepository = repository;
        _bankRepository = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration?.BankRepository ?? new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
        _cardRepository = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration?.CardRepository ?? new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _applicationContext = new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _logger = new Logger<TUser, TCard, TBankAccount, TBank, TCredit>(ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>.Options.LoggerOptions);
    }
    public ExceptionModel Create(TUser item)
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

        Bank bank = null;

        if (_bankRepository.Exist(x => x.ID == item.Card.BankAccount.Bank.ID))
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
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException.Message);
            userCreationTransaction.Rollback();
            throw;
        }

        if (bank is null)
        {
            userCreationTransaction.Commit();
            return ExceptionModel.Successfully;
        }
        item.Card.BankAccount.Bank ??= bank;
        _applicationContext.ChangeTracker.Clear();
        var updateBank = _bankRepository.Update(_bankRepository.Get(x => x.ID == item.Card.BankID));
        if (updateBank != ExceptionModel.Successfully)
        {
            userCreationTransaction.Rollback();
            return updateBank;
        }

        userCreationTransaction.Commit();
        
        return ExceptionModel.Successfully;
    }

    public void Dispose()
    {
        _logger.Log(_reports);
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
        _reports = null;
        _logger = null;
        _disposed = true;
    }

    public ExceptionModel Delete(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationNotExist;
        
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
        var deleteBankAccount = _bankAccountRepository.Delete(_bankAccountRepository.Get(x => x.ID == item.Card.BankAccountID));
        return deleteBankAccount;
    }

    public bool Exist(Expression<Func<TUser, bool>> predicate) => _applicationContext.Users.AsNoTracking().Any(predicate);
    public bool FitsConditions(TUser? item)
    {
        if (item?.Card?.BankAccount?.Bank is null)
            return false;

        if (!Exist(x => x.ID == item.ID))
            return false;

        return true;
    }

    public IEnumerable<TUser> All => _applicationContext.Users.AsNoTracking();

    public TUser? Get(Expression<Func<TUser, bool>> predicate)
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
    
    public ExceptionModel Update(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        
        using var userUpdateTransaction = _applicationContext.Database.BeginTransaction(IsolationLevel.RepeatableRead);
            
        var bankAccountUpdateOperation = _bankAccountRepository.Update(_bankAccountRepository.Get(x => x.ID == item.Card.BankAccountID));
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
        _logger.Log(_reports);
        Dispose(false);
    }
}