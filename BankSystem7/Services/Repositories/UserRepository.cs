using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

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

    private readonly Func<ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>, LambdaExpression, TUser?> GetEntity =
        EF.CompileQuery(
            (ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> context, LambdaExpression predicate) =>
            context.Users
            .Include(x => x.Card)
            .ThenInclude(x => x.BankAccount)
            .ThenInclude(x => x.Bank)
            .AsNoTracking().FirstOrDefault((Expression<Func<TUser, bool>>)predicate));

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
        if (item?.Card?.BankAccount?.Bank is null || item.Equals(User.Default))
            return ExceptionModel.OperationFailed;

        if (Exist(x => x.ID.Equals(item.ID) || x.Name.Equals(item.Name) && x.Email.Equals(item.Email)))
            return ExceptionModel.OperationRestricted;

        UpdateBankTracker(item);
        _applicationContext.Users.Add(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    public ExceptionModel Delete(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.EntityNotExist;

        item.Card.BankAccount.Bank.AccountAmount +=
            _bankRepository.CalculateBankAccountAmount(item.Card.Amount, 0);

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Users.Remove(item);
        _applicationContext.BankAccounts.Remove((TBankAccount)item.Card.BankAccount);
        _applicationContext.Banks.Update((TBank)item.Card.BankAccount.Bank);
        _applicationContext.SaveChanges();

        return ExceptionModel.Ok;
    }

    public bool Exist(Expression<Func<TUser, bool>> predicate)
    {
        return All.Any(predicate);
    }

    public bool FitsConditions(TUser? item)
    {
        return item?.Card?.BankAccount?.Bank is not null && Exist(x => x.ID == item.ID);
    }

    public IQueryable<TUser> All =>
        _applicationContext.Users
            .Include(x => x.Card)
            .ThenInclude(x => x.BankAccount)
            .ThenInclude(x => x.Bank)
            .AsNoTracking() ?? Enumerable.Empty<TUser>().AsQueryable();

    public TUser Get(Expression<Func<TUser, bool>> predicate)
    {
        return All.FirstOrDefault(predicate) ?? (TUser)User.Default;
    }

    public TUser GetCompiled(Expression<Func<User, bool>> predicate)
    {
        try
        {
            var parameters = new ParameterExpression[]
            {
                Expression.Parameter(typeof(TUser)),
            };
            var expression = Expression.Lambda<Func<TUser, bool>>(predicate, parameters);
            return GetEntity(_applicationContext, expression) ?? (TUser)User.Default;
        }
        catch (Exception e)
        {

            throw;
        }
    }

    public ExceptionModel Update(TUser item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        _applicationContext.ChangeTracker.Clear();
        _applicationContext.Users.Update(item);
        _applicationContext.BankAccounts.Update((TBankAccount)item.Card.BankAccount);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    internal ExceptionModel UpdateBankTracker(TUser user)
    {
        _applicationContext.ChangeTracker.Clear();

        var bank = _bankRepository.Get(x => x.ID == user.Card.BankAccount.Bank.ID
                                    || x.BankName == user.Card.BankAccount.Bank.BankName);

        user.Card.BankAccount.Bank.AccountAmount = bank.AccountAmount +=
            _bankRepository.CalculateBankAccountAmount(0, user.Card.Amount);

        if (bank.Equals(Bank.Default))
            return ExceptionModel.IsDefaultValue;

        user.Card.BankAccount.Bank = null;
        _applicationContext.Banks.Update(bank);
        return ExceptionModel.Ok;
    }

    ~UserRepository()
    {
        Dispose(false);
    }
}