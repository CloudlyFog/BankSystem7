using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using BankSystem7.Services.Configuration;

namespace BankSystem7.Services.Repositories;

public sealed class BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit> : IRepository<TBankAccount>, IReaderServiceWithTracking<TBankAccount>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private readonly BankRepository<TUser, TCard, TBankAccount, TBank, TCredit> _bankRepository;
    private readonly BankContext<TUser, TCard, TBankAccount, TBank, TCredit> _bankContext;
    private readonly ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> _applicationContext;
    private bool _disposedValue;
    private const string ConnectionString = @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=Test;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False";

    public BankAccountRepository()
    {
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
        _bankContext = _bankRepository.BankContext;
        SetBankServicesOptions();
        _bankRepository = new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ConnectionString);
    }

    public BankAccountRepository(BankRepository<TUser, TCard, TBankAccount, TBank, TCredit> bankRepository)
    {
        _bankRepository = bankRepository;
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(ServicesSettings.Connection);
        _bankContext = _bankRepository.BankContext;
        SetBankServicesOptions();
    }

    public BankAccountRepository(string connection)
    {
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        _bankContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.BankContext ?? new BankContext<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
        SetBankServicesOptions();
        _bankRepository = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ServiceConfiguration?.BankRepository ?? new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(connection);
    }

    public IQueryable<TBankAccount> All =>
        _applicationContext.BankAccounts
            .Include(x => x.Bank)
            .Include(x => x.User)
            .ThenInclude(x => x.Card)
            .AsNoTracking() ?? Enumerable.Empty<TBankAccount>().AsQueryable();

    public IQueryable<TBankAccount> AllWithTracking =>
        _applicationContext.BankAccounts
            .Include(x => x.Bank)
            .Include(x => x.User)
            .ThenInclude(x => x.Card) ?? Enumerable.Empty<TBankAccount>().AsQueryable();

    public ExceptionModel Transfer(TUser? from, TUser? to, decimal transferAmount)
    {
        if (from is null || to is null || from.Card is null || to.Card is null || from.Card.BankAccount is null ||
            to.Card.BankAccount is null || transferAmount <= 0)
            return ExceptionModel.OperationFailed;

        if (!Exist(x => x.ID == from.Card.BankAccount.ID) || !Exist(x => x.ID == to.Card.BankAccount.ID))
            return ExceptionModel.OperationFailed;

        using var transaction = _bankContext.Database.BeginTransaction(IsolationLevel.RepeatableRead);
        _bankRepository.AnotherBankTransactionOperation = AnotherBankTransactionOperation(from, to);

        var withdraw = Withdraw(from, transferAmount);
        if (withdraw != ExceptionModel.Ok)
        {
            transaction.Rollback();
            return withdraw;
        }

        var accrual = Accrual(to, transferAmount);
        if (accrual != ExceptionModel.Ok)
        {
            transaction.Rollback();
            return accrual;
        }
        transaction.Commit();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// asynchronously accrual money on account with the same user id
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amountAccrual"></param>
    /// <returns>object of <see cref="ExceptionModel"/></returns>
    private ExceptionModel Accrual(TUser? item, decimal amountAccrual)
    {
        CheckBankAccount(item.Card.BankAccount);

        var operation = new Operation
        {
            BankID = item.Card.BankAccount.Bank.ID,
            SenderID = item.Card.BankAccount.Bank.ID,
            ReceiverID = item.ID,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Accrual
        };
        var createOperation = _bankContext.CreateOperation(operation, OperationKind.Accrual);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        var accrualOperation = _bankRepository.BankAccountAccrual(item, operation);
        if (accrualOperation != ExceptionModel.Ok)
            return accrualOperation;

        return ExceptionModel.Ok;
    }

    /// <summary>
    /// withdraw money from account with the same user id
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amountAccrual"></param>
    /// <returns>object of <see cref="ExceptionModel"/></returns>
    private ExceptionModel Withdraw(TUser? item, decimal amountAccrual)
    {
        if (item.Card is null)
            return ExceptionModel.EntityIsNull;

        CheckBankAccount(item.Card.BankAccount);

        var operation = new Operation
        {
            BankID = item.Card.BankAccount.Bank.ID,
            SenderID = item.ID,
            ReceiverID = item.Card.BankAccount.Bank.ID,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Withdraw
        };

        var createOperation = _bankContext.CreateOperation(operation, OperationKind.Withdraw);
        if (createOperation != ExceptionModel.Ok)
            return createOperation;

        var withdraw = _bankRepository.BankAccountWithdraw(item, operation);
        if (withdraw != ExceptionModel.Ok)
            return withdraw;
        return ExceptionModel.Ok;
    }
    public ExceptionModel Create(TBankAccount item)
    {
        if (item is null || Exist(x => x.ID == item.ID) || item.Bank is null)
            return ExceptionModel.EntityIsNull;
        _bankContext.BankAccounts.Add(item);
        _bankContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// updates only BankAccount. Referenced user won't be changed
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public ExceptionModel Update(TBankAccount item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        _applicationContext.ChangeTracker.Clear();
        _applicationContext.BankAccounts.Update(item);
        _bankContext.SaveChanges();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// removes bank account of user from database
    /// </summary>
    /// <param name="item"></param>
    /// <returns>object of <see cref="ExceptionModel"/></returns>
    public ExceptionModel Delete(TBankAccount item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        _applicationContext.ChangeTracker.Clear();
        _applicationContext.BankAccounts.Remove(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Ok;
    }
    
    public TBankAccount Get(Expression<Func<TBankAccount, bool>> predicate)
    {
        return All.FirstOrDefault(predicate) ?? (TBankAccount)BankAccount.Default;
    }
    
    public TBankAccount GetWithTracking(Expression<Func<TBankAccount, bool>> predicate)
    {
        return AllWithTracking.FirstOrDefault(predicate) ?? (TBankAccount)BankAccount.Default;
    }
    
    public bool Exist(Expression<Func<TBankAccount, bool>> predicate)
    {
        return All.Any(predicate);
    }

    public bool ExistWithTracking(Expression<Func<TBankAccount, bool>> predicate)
    {
        return AllWithTracking.Any(predicate);
    }
    
    public bool FitsConditions(TBankAccount? item)
    {
        return item is not null && Exist(x => x.ID == item.ID) && item.Bank is not null;
    }

    private void CheckBankAccount(BankAccount item)
    {
        if (item is null)
            throw new Exception("Passed instance of BankAccount is null.");

        if (!_applicationContext.Users.AsNoTracking().Any(x => x.ID == item.UserID))
            throw new Exception("Doesn't exist user with specified ID in the database.");

        if (!Exist(x => x.ID == item.ID))
            throw new Exception($"Doesn't exist bank with id {{{item.ID}}}");
    }
    
    private void SetBankServicesOptions()
    {
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.BankContext = _bankContext;
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext = _applicationContext;
    }

    private bool AnotherBankTransactionOperation(TUser from, TUser to)
    {
        return from.Card.BankAccount.Bank != to.Card.BankAccount.Bank;
    }

    // Public implementation of Dispose pattern callable by consumers.
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
            _bankContext.Dispose();
            _bankRepository.Dispose();
            _applicationContext.Dispose();
        }
        _disposedValue = true;
    }
    
    ~BankAccountRepository()
    {
        Dispose(false);
    }
}