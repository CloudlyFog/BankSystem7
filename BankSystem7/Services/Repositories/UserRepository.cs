using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BankSystem7.Services.Repositories;

public sealed class UserRepository<TUser, TCard, TBankAccount, TBank, TCredit> : IRepository<TUser>, IRepositoryAsync<TUser>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private readonly BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit> _bankAccountRepository;
    private readonly ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> _applicationContext;
    private readonly BankRepository<TUser, TCard, TBankAccount, TBank, TCredit> _bankRepository;
    private readonly CardRepository<TUser, TCard, TBankAccount, TBank, TCredit> _cardRepository;
    private bool _disposed;

    public UserRepository()
    {
        _bankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
        _bankRepository = new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
        _cardRepository = new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>
                              (ServicesSettings.Connection);
    }

    public UserRepository(string connection)
    {
        _bankAccountRepository = new BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        _bankRepository = new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        _cardRepository = new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>
                              (ServicesSettings.Connection);
    }

    public UserRepository(BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit> repository)
    {
        _bankAccountRepository = repository;
        _bankRepository = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration?.BankRepository ?? new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
        _cardRepository = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration?.CardRepository ?? new CardRepository<TUser, TCard, TBankAccount, TBank, TCredit>(_bankAccountRepository);
        _applicationContext = new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>
            (ServicesSettings.Connection);
    }

    public IQueryable<TUser> All =>
        _applicationContext.Users
            .Include(x => x.Card.BankAccount.Bank)
            .AsNoTracking();

    public IQueryable<TUser> AllWithTracking =>
        _applicationContext.Users
            .Include(x => x.Card.BankAccount.Bank);

    public ExceptionModel Create(TUser item)
    {
        if (item?.Card?.BankAccount?.Bank is null || item.Equals(User.Default))
            return ExceptionModel.OperationFailed;

        if (Exist(x => x.ID.Equals(item.ID) || x.Name.Equals(item.Name) && x.Email.Equals(item.Email)))
            return ExceptionModel.OperationRestricted;

        UpdateBankTracker(item);
        _applicationContext.Users.Add(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> CreateAsync(TUser item)
    {
        if (item?.Card?.BankAccount?.Bank is null || item.Equals(User.Default))
            return ExceptionModel.OperationFailed;

        if (Exist(x => x.ID.Equals(item.ID) || x.Name.Equals(item.Name) && x.Email.Equals(item.Email)))
            return ExceptionModel.OperationRestricted;

        UpdateBankTracker(item);
        _applicationContext.Users.Add(item);
        await _applicationContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    public ExceptionModel Update(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.UpdateRange(item, item.Card.BankAccount);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> UpdateAsync(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.UpdateRange(item, item.Card.BankAccount);
        await _applicationContext.SaveChangesAsync();
        return ExceptionModel.Ok;
    }

    public ExceptionModel Delete(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.EntityNotExist;

        item.Card.BankAccount.Bank.AccountAmount +=
            _bankRepository.CalculateBankAccountAmount(item.Card.Amount, 0);

        _applicationContext.Users.Remove(item);
        _applicationContext.BankAccounts.Remove((TBankAccount)item.Card.BankAccount);
        _applicationContext.Banks.Update((TBank)item.Card.BankAccount.Bank);
        _applicationContext.SaveChanges();

        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> DeleteAsync(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.EntityNotExist;

        item.Card.BankAccount.Bank.AccountAmount +=
            _bankRepository.CalculateBankAccountAmount(item.Card.Amount, 0);

        _applicationContext.Users.Remove(item);
        _applicationContext.BankAccounts.Remove((TBankAccount)item.Card.BankAccount);
        _applicationContext.Banks.Update((TBank)item.Card.BankAccount.Bank);
        await _applicationContext.SaveChangesAsync();

        return ExceptionModel.Ok;
    }

    public bool Exist(Expression<Func<TUser, bool>> predicate)
    {
        return All.Any(predicate);
    }

    public async Task<bool> ExistAsync(Expression<Func<TUser, bool>> predicate)
    {
        return await All.AnyAsync(predicate);
    }

    public bool FitsConditions(TUser? item)
    {
        return item?.Card?.BankAccount?.Bank is not null && Exist(x => x.ID == item.ID);
    }

    public async Task<bool> FitsConditionsAsync(TUser? item)
    {
        return item?.Card?.BankAccount?.Bank is not null && await ExistAsync(x => x.ID == item.ID);
    }

    public TUser Get(Expression<Func<TUser, bool>> predicate)
    {
        return All.FirstOrDefault(predicate) ?? (TUser)User.Default;
    }

    public async Task<TUser> GetAsync(Expression<Func<TUser, bool>> predicate)
    {
        return await All.FirstOrDefaultAsync(predicate) ?? (TUser)User.Default;
    }

    private void UpdateBankTracker(TUser user)
    {
        _applicationContext.ChangeTracker.Clear();

        var bank = _bankRepository.Get(x => x.ID == user.Card.BankAccount.Bank.ID
                                    || x.BankName == user.Card.BankAccount.Bank.BankName);

        user.Card.BankAccount.Bank.AccountAmount = bank.AccountAmount +=
            _bankRepository.CalculateBankAccountAmount(0, user.Card.Amount);

        if (bank.Equals(Bank.Default))
            return;

        user.Card.BankAccount.Bank = null;
        _applicationContext.Banks.Update(bank);
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
        _disposed = true;
    }

    ~UserRepository()
    {
        Dispose(false);
    }
}