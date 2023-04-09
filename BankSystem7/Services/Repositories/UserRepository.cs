using System.Data;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Repositories;


public sealed class UserRepository : LoggerExecutor<OperationType>, IRepository<User>
{
    private BankAccountRepository _bankAccountRepository;
    private ApplicationContext _applicationContext;
    private BankRepository _bankRepository;
    private CardRepository _cardRepository;
    private ILogger _logger;
    private List<GeneralReport<OperationType>> _reports = new();
    private bool _disposed;
    public UserRepository()
    {
        _bankAccountRepository = new BankAccountRepository(BankServicesOptions.Connection);
        _bankRepository = new BankRepository(BankServicesOptions.Connection);
        _cardRepository = new CardRepository(_bankAccountRepository);
        _applicationContext = BankServicesOptions.ApplicationContext ??
                              new ApplicationContext(_bankAccountRepository);
        _logger = new Logger(ServiceConfiguration.Options.LoggerOptions);
    }
    public UserRepository(string connection)
    {
        _bankAccountRepository = new BankAccountRepository(connection);
        _bankRepository = new BankRepository(connection);
        _cardRepository = new CardRepository(_bankAccountRepository);
        _applicationContext = BankServicesOptions.ApplicationContext ??
                              new ApplicationContext(_bankAccountRepository);
        _logger = new Logger(ServiceConfiguration.Options.LoggerOptions);
        
    }
    public UserRepository(BankAccountRepository repository)
    {
        _bankAccountRepository = repository;
        _bankRepository = BankServicesOptions.ServiceConfiguration?.BankRepository ?? new BankRepository(BankServicesOptions.Connection);
        _cardRepository = BankServicesOptions.ServiceConfiguration?.CardRepository ?? new CardRepository(_bankAccountRepository);
        _applicationContext = new ApplicationContext(_bankAccountRepository);
        _logger = new Logger(ServiceConfiguration.Options.LoggerOptions);
    }
    public ExceptionModel Create(User item)
    {
        if (item?.Card?.BankAccount?.Bank is null)
        {
            Log(ExceptionModel.VariableIsNull, nameof(Create), nameof(UserRepository), OperationType.Create, _reports);
            return ExceptionModel.OperationFailed;
        }
            
        //if user exists method will send false
        if (Exist(x => x.ID == item.ID))
        {
            Log(ExceptionModel.OperationNotExist, nameof(Create), nameof(UserRepository), OperationType.Create, _reports);
            return ExceptionModel.OperationFailed;
        }

        using var userCreationTransaction = _applicationContext.Database.BeginTransaction(IsolationLevel
                                                .RepeatableRead);
            
        
        var avoidDuplication = _applicationContext.AvoidDuplication(item.Card.BankAccount.Bank);
        if (avoidDuplication != ExceptionModel.Successfully)
        {
            userCreationTransaction.Rollback();
            Log(avoidDuplication, nameof(Create), nameof(UserRepository), OperationType.Create, _reports);
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
            Log(ExceptionModel.Successfully, nameof(Create), nameof(UserRepository), OperationType.Create, _reports);
            return ExceptionModel.Successfully;
        }
        item.Card.BankAccount.Bank ??= bank;
        _applicationContext.ChangeTracker.Clear();
        var updateBank = _bankRepository.Update(item.Card.BankAccount.Bank);
        if (updateBank != ExceptionModel.Successfully)
        {
            userCreationTransaction.Rollback();
            
            Log(updateBank, nameof(Create), nameof(UserRepository), OperationType.Create, _reports);
            return updateBank;
        }

        userCreationTransaction.Commit();
        
        
        Log(ExceptionModel.Successfully, nameof(Create), nameof(UserRepository), OperationType.Create, _reports);
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

    public ExceptionModel Delete(User item)
    {
        if (!FitsConditions(item))
        {
            Log(ExceptionModel.OperationNotExist, nameof(Delete), nameof(UserRepository), OperationType.Delete, _reports);
            return ExceptionModel.OperationNotExist;
        }
        
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
        var deleteBankAccount = _bankAccountRepository.Delete(item.Card.BankAccount);

        Log(deleteBankAccount, nameof(Delete), nameof(UserRepository), OperationType.Delete, _reports);
        return deleteBankAccount;
    }

    public bool Exist(Expression<Func<User, bool>> predicate) => _applicationContext.Users.AsNoTracking().Any(predicate);
    public bool FitsConditions(User? item)
    {
        if (item?.Card?.BankAccount?.Bank is null)
        {
            Log(ExceptionModel.VariableIsNull, nameof(FitsConditions), nameof(UserRepository), OperationType.FitsConditions, _reports);
            return false;
        }

        if (!Exist(x => x.ID == item.ID))
        {
            Log(ExceptionModel.OperationNotExist, nameof(FitsConditions), nameof(UserRepository), OperationType.FitsConditions, _reports);
            return false;
        }

        return true;
    }

    public IEnumerable<User> All
    {
        get
        {
            Log(ExceptionModel.Successfully, nameof(All), nameof(UserRepository), OperationType.All, _reports);
            return _applicationContext.Users.AsNoTracking();
        }
    }

    public User? Get(Expression<Func<User, bool>> predicate)
    {
        var user = _applicationContext.Users.AsNoTracking().FirstOrDefault(predicate);
        if (user is null)
        {
            Log(ExceptionModel.VariableIsNull, nameof(Get), nameof(UserRepository), OperationType.Read, _reports);
            return user;
        }
        user.Card = _cardRepository.Get(x => x.UserID == user.ID);
        if (user.Card is null)
        {
            Log(ExceptionModel.VariableIsNull, nameof(Get), nameof(UserRepository), OperationType.Read, _reports);
            return user;
        }
        user.Card.BankAccount = _bankAccountRepository.Get(x => x.UserID == user.ID);
        if (user.Card.BankAccount is null)
        {
            Log(ExceptionModel.VariableIsNull, nameof(Get), nameof(UserRepository), OperationType.Read, _reports);
            return user;
        }
        user.Card.BankAccount.Bank = _bankRepository.Get(x => x.BankAccounts.Contains(user.Card.BankAccount));
        if (user.Card.BankAccount.Bank is null)
        {
            Log(ExceptionModel.VariableIsNull, nameof(Get), nameof(UserRepository), OperationType.Read, _reports);
            return user;
        }
        
        Log(ExceptionModel.Successfully, nameof(Get), nameof(UserRepository), OperationType.Read, _reports);
        return user;
    }
    
    public ExceptionModel Update(User item)
    {
        if (!FitsConditions(item))
        {
            Log(ExceptionModel.OperationFailed, nameof(Update), nameof(UserRepository), OperationType.Update, _reports);
            return ExceptionModel.OperationFailed;
        }
        using var userUpdateTransaction = _applicationContext.Database.BeginTransaction(IsolationLevel.RepeatableRead);
            
        var bankAccountDeletionOperation = _bankAccountRepository.Update(item.Card.BankAccount);
        if (bankAccountDeletionOperation != ExceptionModel.Successfully)
        {
            userUpdateTransaction.Rollback();
            Log(bankAccountDeletionOperation, nameof(Update), nameof(UserRepository), OperationType.Update, _reports);
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
        Log(ExceptionModel.Successfully, nameof(Update), nameof(UserRepository), OperationType.Update, _reports);
        return ExceptionModel.Successfully;
    }

    ~UserRepository()
    {
        _logger.Log(_reports);
        Dispose(false);
    }
}