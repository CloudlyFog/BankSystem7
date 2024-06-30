using BankSystem7.BankAggregate;
using BankSystem7.BankAggregate.BankAccountAggregate;
using BankSystem7.BankAggregate.CardAggregate;
using BankSystem7.BankAggregate.OperationAggregate;
using BankSystem7.Configuration;
using BankSystem7.UserAggregate;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace BankSystem7.ApplicationAggregate.Data;

public sealed class BankContext : GenericDbContext
{
    private readonly OperationService<Operation> _operationService;
    public BankContext(ConfigurationOptions options) : base(options)
    {
        _operationService = new OperationService<Operation>();
    }

    public BankContext(DbContextOptions dbContextOptions, ConfigurationOptions options) : base(dbContextOptions, options)
    {
        _operationService = new OperationService<Operation>();
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Bank> Banks { get; set; } = null!;
    public DbSet<BankAccount> BankAccounts { get; set; } = null!;
    public DbSet<Card> Cards { get; set; } = null!;

    /// <summary>
    /// creates transaction operation
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="operationKind"></param>
    public ExceptionModel CreateOperation(Operation? operation, OperationKind operationKind)
    {
        if (operation is null)
            return ExceptionModel.EntityIsNull;

        // Find(Builders<Operation>.Filter.Eq(predicate)).Any() equals
        // Operations.Any(predicate)
        // we find and in the same time check is there object in database
        if (_operationService.Collection.Find(Builders<Operation>.Filter.Eq(x => x.Id, operation.Id)).Any())
            return ExceptionModel.EntityNotExist;

        operation.OperationStatus = GetStatusOperation(operation, operationKind);
        if (operation.OperationStatus is not StatusOperationCode.Ok or StatusOperationCode.Successfully)
            return ExceptionModel.OperationRestricted;

        _operationService.Collection.InsertOne(operation);
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// creates transaction operation
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="operationKind"></param>
    public async Task<ExceptionModel> CreateOperationAsync(Operation? operation, OperationKind operationKind, CancellationToken cancellationToken = default)
    {
        if (operation is null)
            return ExceptionModel.EntityIsNull;

        // Find(Builders<Operation>.Filter.Eq(predicate)).Any() equals
        // Operations.Any(predicate)
        // we find and in the same time check is there object in database
        if (await _operationService.Collection.Find(Builders<Operation>.Filter.Eq(x => x.Id, operation.Id)).AnyAsync(cancellationToken))
            return ExceptionModel.EntityNotExist;

        operation.OperationStatus = GetStatusOperation(operation, operationKind);
        if (operation.OperationStatus is not StatusOperationCode.Ok or StatusOperationCode.Successfully)
            return ExceptionModel.OperationRestricted;

        await _operationService.Collection.InsertOneAsync(operation, cancellationToken: cancellationToken);
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// delete transaction operation
    /// </summary>
    /// <param name="operation"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ExceptionModel DeleteOperation(Operation? operation)
    {
        if (operation is null)
            return ExceptionModel.EntityIsNull;
        if (_operationService.Collection.Find(Builders<Operation>.Filter.Eq(x => x.Id, operation.Id)).Any())
            return ExceptionModel.EntityNotExist;

        _operationService.Collection.DeleteOne(x => x.Id == operation.Id);
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// delete transaction operation
    /// </summary>
    /// <param name="operation"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<ExceptionModel> DeleteOperationAsync(Operation? operation, CancellationToken cancellationToken = default)
    {
        if (operation is null)
            return ExceptionModel.EntityIsNull;
        if (await _operationService.Collection.Find(Builders<Operation>.Filter.Eq(x => x.Id, operation.Id)).AnyAsync(cancellationToken: cancellationToken))
            return ExceptionModel.EntityNotExist;

        await _operationService.Collection.DeleteOneAsync(x => x.Id == operation.Id, cancellationToken: cancellationToken);
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// withdraw money from user bank account and accrual to bank's account
    /// </summary>
    /// <param name="user"></param>
    /// <param name="bank"></param>
    /// <param name="operation"></param>
    /// <exception cref="Exception"></exception>
    internal ExceptionModel BankAccountWithdraw(User? user, Bank? bank, Operation operation)
    {
        if (user.Card?.BankAccount?.Bank is null || bank is null || operation is null)
            return ExceptionModel.EntityIsNull;
        if (operation.OperationStatus != StatusOperationCode.Ok)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        bank.AccountAmount += operation.TransferAmount;
        user.Card.BankAccount.BankAccountAmount -= operation.TransferAmount;
        user.Card.Amount -= operation.TransferAmount;

        UpdateRange(user.Card, user.Card.BankAccount, user.Card.BankAccount.Bank);
        SaveChanges();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// withdraw money from user bank account and accrual to bank's account
    /// </summary>
    /// <param name="user"></param>
    /// <param name="bank"></param>
    /// <param name="operation"></param>
    /// <exception cref="Exception"></exception>
    internal async Task<ExceptionModel> BankAccountWithdrawAsync(User? user, Bank? bank, Operation operation, CancellationToken cancellationToken = default)
    {
        if (user.Card?.BankAccount?.Bank is null || bank is null || operation is null)
            return ExceptionModel.EntityIsNull;
        if (operation.OperationStatus != StatusOperationCode.Ok)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        bank.AccountAmount += operation.TransferAmount;
        user.Card.BankAccount.BankAccountAmount -= operation.TransferAmount;
        user.Card.Amount -= operation.TransferAmount;

        UpdateRange(user.Card, user.Card.BankAccount, user.Card.BankAccount.Bank);
        await SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// accrual money to user bank account from bank's account
    /// </summary>
    /// <param name="user"></param>
    /// <param name="bank"></param>
    /// <param name="operation"></param>
    /// <exception cref="Exception"></exception>
    internal ExceptionModel BankAccountAccrual(User? user, Bank? bank, Operation operation)
    {
        if (user.Card?.BankAccount?.Bank is null || bank is null || operation is null)
            return ExceptionModel.EntityIsNull;
        if (operation.OperationStatus != StatusOperationCode.Ok)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        bank.AccountAmount -= operation.TransferAmount;
        user.Card.BankAccount.BankAccountAmount += operation.TransferAmount;
        user.Card.Amount += operation.TransferAmount;

        UpdateRange(user.Card, user.Card.BankAccount, user.Card.BankAccount.Bank);
        SaveChanges();
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// accrual money to user bank account from bank's account
    /// </summary>
    /// <param name="user"></param>
    /// <param name="bank"></param>
    /// <param name="operation"></param>
    /// <exception cref="Exception"></exception>
    internal async Task<ExceptionModel> BankAccountAccrualAsync(User? user, Bank? bank, Operation operation, CancellationToken cancellationToken = default)
    {
        if (user.Card?.BankAccount?.Bank is null || bank is null || operation is null)
            return ExceptionModel.EntityIsNull;
        if (operation.OperationStatus != StatusOperationCode.Ok)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        bank.AccountAmount -= operation.TransferAmount;
        user.Card.BankAccount.BankAccountAmount += operation.TransferAmount;
        user.Card.Amount += operation.TransferAmount;

        UpdateRange(user.Card, user.Card.BankAccount, user.Card.BankAccount.Bank);
        await SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    /// <summary>
    /// check:
    /// 1) is exist user with the same ID and bank with the same BankID as a sender or reciever in the database.
    /// 2) is exist bank with the same BankID as a single bank.
    /// 3) is bank's money enough for transaction.
    /// 4) is user's money enough for transaction.
    /// </summary>
    /// <param name="operationModel"></param>
    /// <param name="operationKind"></param>
    /// <returns>status of operation, default - Ok or Successfully</returns>
    /// <exception cref="ArgumentNullException"></exception>
    private StatusOperationCode GetStatusOperation(Operation? operationModel, OperationKind operationKind)
    {
        if (operationModel is null || !Banks.AsNoTracking().Any(x => x.Id == operationModel.BankId))
            return StatusOperationCode.Error;

        if (operationKind == OperationKind.Accrual)
        {
            // Check if the receiver ID exists in the Users table without tracking changes
            if (!Users.AsNoTracking().Select(x => x.Id).Any(x => x == operationModel.ReceiverId))
                return StatusOperationCode.Error;

            // Check if the sender ID exists in the Banks table and if the account amount is less than the transfer amount
            if (Banks.AsNoTracking().FirstOrDefault(x => x.Id == operationModel.SenderId)?.AccountAmount < operationModel.TransferAmount)
                return StatusOperationCode.Restricted;
        }
        else if (operationKind == OperationKind.Withdraw)
        {
            // Check if the SenderID of the operationModel exists in the Users table
            if (!Users.AsNoTracking().Select(x => x.Id).Any(x => x == operationModel.SenderId))
                return StatusOperationCode.Error;

            // Check if the SenderID of the operationModel has enough funds in their bank account
            if (BankAccounts.AsNoTracking().FirstOrDefault(x => x.UserId == operationModel.SenderId)?.BankAccountAmount < operationModel.TransferAmount)
                return StatusOperationCode.Restricted;
        }

        return StatusOperationCode.Ok;
    }
}