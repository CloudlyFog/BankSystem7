using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;

namespace BankSystem7.Services.Repositories;

public sealed class BankAccountRepository : IRepository<BankAccount>
{
    private BankRepository _bankRepository;
    private BankContext _bankContext;
    private ApplicationContext _applicationContext;
    private bool _disposedValue;
    private const string ConnectionString = @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=Test;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False";

    public BankAccountRepository()
    {
        _bankContext = _bankRepository.BankContext;
        _applicationContext = BankServicesOptions.ApplicationContext ??
                              new ApplicationContext(BankServicesOptions.Connection);
        SetBankServicesOptions();
        _bankRepository = new BankRepository(ConnectionString);
    }
    public BankAccountRepository(BankRepository bankRepository)
    {
        _bankRepository = bankRepository;
        _bankContext = _bankRepository.BankContext;
        _applicationContext = BankServicesOptions.ApplicationContext ??
                              new ApplicationContext(BankServicesOptions.Connection);
        SetBankServicesOptions();
    }
    public BankAccountRepository(string connection)
    {
        _bankContext = BankServicesOptions.BankContext ?? new BankContext(connection);
        _applicationContext = BankServicesOptions.ApplicationContext ??
                              new ApplicationContext(connection);
        SetBankServicesOptions();
        _bankRepository = BankServicesOptions.ServiceConfiguration?.BankRepository ?? new BankRepository(connection);
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

    public async Task<ExceptionModel> Transfer(User? from, User? to, decimal transferAmount, decimal minimalTransferAmount = 0)
    {
        if (from is null || to is null || from.Card is null || to.Card is null || from.Card.BankAccount is null ||
            to.Card.BankAccount is null || transferAmount <= minimalTransferAmount)
            return ExceptionModel.OperationFailed;
            
        if (!Exist(x => x.ID == from.Card.BankAccount.ID) || !Exist(x => x.ID == to.Card.BankAccount.ID))
            return ExceptionModel.OperationFailed;
            
        using var transaction = _bankContext.Database.BeginTransaction(IsolationLevel.RepeatableRead);
        _bankRepository.AnotherBankTransactionOperation = AnotherBankTransactionOperation(from, to);
        try
        {
            await WithdrawAsync(from, transferAmount);
            await AccrualAsync(to, transferAmount);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            transaction.Rollback();
            throw;
        }
        transaction.Commit();
        return ExceptionModel.Successfully;
    }
        
    public async Task<ExceptionModel> Transfer(BankAccount? from, BankAccount? to, decimal transferAmount)
    {
        if (from is null || to is null || transferAmount <= 0)
            return ExceptionModel.OperationFailed;
            
        if (!Exist(x => x.ID == from.ID) || !Exist(x => x.ID == to.ID))
            return ExceptionModel.OperationFailed;
            
        using var transaction = _bankContext.Database.BeginTransaction(IsolationLevel.RepeatableRead);
        await WithdrawAsync(from, transferAmount);
        await AccrualAsync(to, transferAmount);
        transaction.Commit();
        return ExceptionModel.Successfully;
    }

    /// <summary>
    /// asynchronously accrual money on account with the same user id
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amountAccrual"></param>
    /// <returns>object of <see cref="ExceptionModel"/></returns>
    private async Task AccrualAsync(BankAccount? item, decimal amountAccrual)
    {
        CheckBankAccount(item);

        var operation = new Operation
        {
            BankID = item.BankID,
            SenderID = item.BankID,
            ReceiverID = item.UserID,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Accrual
        };
        var createOperation = _bankContext.CreateOperation(operation, OperationKind.Accrual);
            
        if (createOperation != ExceptionModel.Successfully)
            throw new Exception($"Operation can't create due to exception: {createOperation}");
            
        var bank = _bankRepository.Get(x => x.ID == operation.BankID);
        if (_bankRepository.BankAccountAccrual(item, bank, operation) != ExceptionModel.Successfully)
            throw new Exception($"Failed withdraw money from {bank}");
    }
        
    /// <summary>
    /// asynchronously accrual money on account with the same user id
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amountAccrual"></param>
    /// <returns>object of <see cref="ExceptionModel"/></returns>
    private async Task AccrualAsync(User? item, decimal amountAccrual)
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
            throw new Exception($"Operation can't create due to exception: {createOperation}");
            
        if (_bankRepository.BankAccountAccrual(item, operation) != ExceptionModel.Successfully)
            throw new Exception($"Failed withdraw money from {item.Card.BankAccount.Bank.BankName}");
    }

    /// <summary>
    /// withdraw money from account with the same user id
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amountAccrual"></param>
    /// <returns>object of <see cref="ExceptionModel"/></returns>
    private async Task WithdrawAsync(BankAccount? item, decimal amountAccrual)
    {
        CheckBankAccount(item);

        var operation = new Operation
        {
            BankID = item.BankID,
            SenderID = item.UserID,
            ReceiverID = item.BankID,
            TransferAmount = amountAccrual,
            OperationKind = OperationKind.Withdraw
        };
            
        var createOperation = _bankContext.CreateOperation(operation, OperationKind.Withdraw);
        if (createOperation != ExceptionModel.Successfully)
            throw new Exception($"Operation can't create due to exception: {createOperation}");
            
        var bank = _bankRepository.Get(x => x.ID == operation.BankID);
        var withdraw = _bankRepository.BankAccountWithdraw(item, bank, operation);
        if (withdraw != ExceptionModel.Successfully)
            throw new Exception($"Failed withdraw money from {bank}\nException: {withdraw}");
    }
        
    /// <summary>
    /// withdraw money from account with the same user id
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amountAccrual"></param>
    /// <returns>object of <see cref="ExceptionModel"/></returns>
    private async Task WithdrawAsync(User? item, decimal amountAccrual)
    {
        if (item.Card is null)
            throw new Exception("Passed instance of Card is null.");
            
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
            throw new Exception($"Operation can't create due to exception: {createOperation}");
            
        var withdraw = _bankRepository.BankAccountWithdraw(item, operation);
        if (withdraw != ExceptionModel.Successfully)
            throw new Exception($"Failed withdraw money from {item.Card.BankAccount.Bank.BankName}\nException: {withdraw}");
    }

    public ExceptionModel Create(BankAccount item)
    {
        if (item is null || Exist(x => x.ID == item.ID) || item.Bank is null)
            return ExceptionModel.VariableIsNull;
        _bankRepository.Update(item.Bank);
        _bankContext.BankAccounts.Add(item);
        _bankContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public IEnumerable<BankAccount> All => _applicationContext.BankAccounts.AsNoTracking();

    public BankAccount? Get(Expression<Func<BankAccount, bool>> predicate) => _applicationContext.BankAccounts.AsNoTracking().FirstOrDefault(predicate);

    /// <summary>
    /// updates only BankAccount. Referenced user won't be changed
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public ExceptionModel Update(BankAccount item)
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
    public ExceptionModel Delete(BankAccount item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        _applicationContext.ChangeTracker.Clear();
        _applicationContext.BankAccounts.Remove(item);
        _applicationContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public bool Exist(Expression<Func<BankAccount, bool>> predicate)
        => _applicationContext.BankAccounts.AsNoTracking().Any(predicate);

    public bool FitsConditions(BankAccount? item)
    {
        return item is not null && Exist(x => x.ID == item.ID) && item.Bank is not null;
    }

    public ExceptionModel Update(BankAccount item, User user, Card card)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        if (user is null || !_bankRepository.Exist(x => x.ID == user.ID))
            return ExceptionModel.VariableIsNull;
            
        if (card is null || !_bankContext.Cards.AsNoTracking().Any(x => x.ID == card.ID))
            return ExceptionModel.VariableIsNull;

        _applicationContext.ChangeTracker.Clear();
        using var transaction = _applicationContext.Database.BeginTransaction();
        _applicationContext.BankAccounts.Update(item);
        _applicationContext.Users.Update(user);
        _applicationContext.SaveChanges(); 
        transaction.Commit();

        return ExceptionModel.Successfully;
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
    
    private bool AnotherBankTransactionOperation(User from, User to)
    {
        return from.Card.BankAccount.Bank != to.Card.BankAccount.Bank;
    }

    private void SetBankServicesOptions()
    {
        BankServicesOptions.BankContext = _bankContext;
        BankServicesOptions.ApplicationContext = _applicationContext;
    }

    ~BankAccountRepository()
    {
        Dispose(false);
    }
}