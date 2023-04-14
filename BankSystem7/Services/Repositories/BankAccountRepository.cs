using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;

namespace BankSystem7.Services.Repositories;

public sealed class BankAccountRepository<TUser, TCard, TBankAccount, TBank, TCredit> : IRepository<TBankAccount>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private BankRepository<TUser, TCard, TBankAccount, TBank, TCredit> _bankRepository;
    private BankContext<TUser, TCard, TBankAccount, TBank, TCredit> _bankContext;
    private ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit> _applicationContext;
    private bool _disposedValue;
    private const string ConnectionString = @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=Test;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False";

    public BankAccountRepository()
    {
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
        _bankContext = _bankRepository.BankContext;
        SetBankServicesOptions();
        _bankRepository = new BankRepository<TUser, TCard, TBankAccount, TBank, TCredit>(ConnectionString);
    }

    public BankAccountRepository(BankRepository<TUser, TCard, TBankAccount, TBank, TCredit> bankRepository)
    {
        _bankRepository = bankRepository;
        _applicationContext = BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext ??
                              new ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>(BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.Connection);
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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _bankContext = null;
        _bankRepository = null;
        _applicationContext = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        _disposedValue = true;
    }

    public async Task<ExceptionModel> Transfer(TUser? from, TUser? to, decimal transferAmount)
    {
        if (from is null || to is null || from.Card is null || to.Card is null || from.Card.BankAccount is null ||
            to.Card.BankAccount is null || transferAmount <= 0)
            return ExceptionModel.OperationFailed;

        if (!Exist(x => x.ID == from.Card.BankAccount.ID) || !Exist(x => x.ID == to.Card.BankAccount.ID))
            return ExceptionModel.OperationFailed;

        using var transaction = _bankContext.Database.BeginTransaction(IsolationLevel.RepeatableRead);
        _bankRepository.AnotherBankTransactionOperation = AnotherBankTransactionOperation(from, to);

        var withdraw = Withdraw(from, transferAmount);
        if (withdraw != ExceptionModel.Successfully)
        {
            transaction.Rollback();
            return withdraw;
        }

        var accrual = Accrual(to, transferAmount);
        if (accrual != ExceptionModel.Successfully)
        {
            transaction.Rollback();
            return accrual;
        }
        transaction.Commit();
        return ExceptionModel.Successfully;
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
            BankID = item.BankID,
            SenderID = item.BankID,
            ReceiverID = item.ID,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Accrual
        };
        var createOperation = _bankContext.CreateOperation(operation, OperationKind.Accrual);
        if (createOperation != ExceptionModel.Successfully)
            return createOperation;

        var accrualOperation = _bankRepository.BankAccountAccrual(item, operation);
        if (accrualOperation != ExceptionModel.Successfully)
            return accrualOperation;

        return ExceptionModel.Successfully;
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
            return ExceptionModel.VariableIsNull;

        CheckBankAccount(item.Card.BankAccount);

        var operation = new Operation
        {
            BankID = item.BankID,
            SenderID = item.ID,
            ReceiverID = item.BankID,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Withdraw
        };

        var createOperation = _bankContext.CreateOperation(operation, OperationKind.Withdraw);
        if (createOperation != ExceptionModel.Successfully)
            return createOperation;

        var withdraw = _bankRepository.BankAccountWithdraw(item, operation);
        if (withdraw != ExceptionModel.Successfully)
            return withdraw;
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Create(TBankAccount item)
    {
        if (item is null || Exist(x => x.ID == item.ID) || item.Bank is null)
            return ExceptionModel.VariableIsNull;
        _bankContext.BankAccounts.Add(item);
        _bankContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public IEnumerable<TBankAccount> All => _applicationContext.BankAccounts.Include(x => x.Bank).AsNoTracking();

    public TBankAccount? Get(Expression<Func<TBankAccount, bool>> predicate)
    {
        return _applicationContext.BankAccounts.Include(x => x.Bank)
            .AsNoTracking().FirstOrDefault(predicate);
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
        return ExceptionModel.Successfully;
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
        return ExceptionModel.Successfully;
    }

    public bool Exist(Expression<Func<TBankAccount, bool>> predicate)
        => _applicationContext.BankAccounts.AsNoTracking().Any(predicate);

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

    private bool AnotherBankTransactionOperation(TUser from, TUser to)
    {
        return from.Card.BankAccount.Bank != to.Card.BankAccount.Bank;
    }

    private void SetBankServicesOptions()
    {
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.BankContext = _bankContext;
        BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>.ApplicationContext = _applicationContext;
    }

    ~BankAccountRepository()
    {
        Dispose(false);
    }
}