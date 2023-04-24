using BankSystem7.AppContext;
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

        if (Exist(x => x.ID.Equals(item.ID) || x.Name.Equals(item.Name) && x.Email.Equals(item.Email)))
            return ExceptionModel.OperationRestricted;


        _applicationContext.ChangeTracker.Clear();

        item.Card.BankAccount.Bank.AccountAmount = 
            _bankRepository.CalculateBankAmount(item.Card.BankAccount.Bank.ID, 0, item.Card.Amount);

        if (!_bankRepository.Exist(x => x.ID == item.Card.BankAccount.Bank.ID || x.BankName == item.Card.BankAccount.Bank.BankName))
        {
            _applicationContext.Users.Add(item);
            _applicationContext.SaveChanges();
            return ExceptionModel.Successfully;
        }

        _applicationContext.Entry(item.Card.BankAccount.Bank).State = EntityState.Unchanged;
        _applicationContext.Users.Add(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationNotExist;

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Users.Remove(item);
        _applicationContext.BankAccounts.Remove((TBankAccount)item.Card.BankAccount);
        try
        {
            _applicationContext.SaveChanges();
        }
        catch (Exception)
        {
            return ExceptionModel.ThrewException;
        }

        return ExceptionModel.Successfully;
    }

    public bool Exist(Func<TUser, bool> predicate)
    {
        return All.Any(predicate);
    }

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
        return All.FirstOrDefault(predicate) ?? (TUser)User.Default;
    }

    public ExceptionModel Update(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Users.Update(item);
        _applicationContext.BankAccounts.Update((TBankAccount)item.Card.BankAccount);
        try
        {
            _applicationContext.SaveChanges();
        }
        catch (Exception)
        {
            return ExceptionModel.ThrewException;
        }
        return ExceptionModel.Successfully;
    }


    ~UserRepository()
    {
        Dispose(false);
    }
}