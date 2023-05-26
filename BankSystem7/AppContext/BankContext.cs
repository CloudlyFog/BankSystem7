using BankSystem7.Models;
using BankSystem7.Services;
using BankSystem7.Services.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MongoDB.Driver;

namespace BankSystem7.AppContext;

internal sealed class BankContext<TUser, TCard, TBankAccount, TBank, TCredit> : GenericDbContext
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private readonly OperationService<Operation> _operationService;

    public BankContext() : base()
    {
        _operationService = new OperationService<Operation>();
    }

    public BankContext(string connection) : base(connection)
    {
        ServicesSettings.SetConnection(connection);
        _operationService = new OperationService<Operation>(ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>.Options.LoggerOptions?.OperationServiceOptions?.DatabaseName ?? "CabManagementSystemReborn");
    }

    public DbSet<TUser> Users { get; set; } = null!;
    public DbSet<Bank> Banks { get; set; } = null!;
    public DbSet<BankAccount> BankAccounts { get; set; } = null!;
    public DbSet<Card> Cards { get; set; } = null!;

    public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
    {
        var entry = Entry(entity);
        if (entry.State != EntityState.Modified)
            return base.Update(entity);
        return entry;
    }

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
        if (_operationService.Collection.Find(Builders<Operation>.Filter.Eq(x => x.ID, operation.ID)).Any())
            return ExceptionModel.EntityNotExist;

        operation.OperationStatus = StatusOperation(operation, operationKind);
        if (operation.OperationStatus != StatusOperationCode.Ok)
            return ExceptionModel.OperationRestricted;

        _operationService.Collection.InsertOne(operation);
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
        if (_operationService.Collection.Find(Builders<Operation>.Filter.Eq(x => x.ID, operation.ID)).Any())
            return ExceptionModel.EntityNotExist;

        _operationService.Collection.DeleteOne(x => x.ID == operation.ID);
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
        ChangeTracker.Clear();
        Update(user.Card);
        Update(user.Card.BankAccount);
        Update(user.Card.BankAccount.Bank);
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
    internal ExceptionModel BankAccountAccrual(User? user, Bank? bank, Operation operation)
    {
        if (user.Card?.BankAccount?.Bank is null || bank is null || operation is null)
            return ExceptionModel.EntityIsNull;
        if (operation.OperationStatus != StatusOperationCode.Ok)
            return (ExceptionModel)operation.OperationStatus.GetHashCode();

        bank.AccountAmount -= operation.TransferAmount;
        user.Card.BankAccount.BankAccountAmount += operation.TransferAmount;
        user.Card.Amount += operation.TransferAmount;
        ChangeTracker.Clear();
        Update(user.Card);
        Update(user.Card.BankAccount);
        Update(user.Card.BankAccount.Bank);
        SaveChanges();
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
    private StatusOperationCode StatusOperation(Operation? operationModel, OperationKind operationKind)
    {
        if (operationModel is null)
            return StatusOperationCode.Error;

        if (operationKind == OperationKind.Accrual)
        {
            // SenderID is ID of bank
            // ReceiverID is ID of user
            if (!Banks.AsNoTracking().Any(x => x.ID == operationModel.SenderID) || !Users.Any(x => x.ID == operationModel.ReceiverID))
                operationModel.OperationStatus = StatusOperationCode.Error;

            if (Banks.AsNoTracking().FirstOrDefault(x => x.ID == operationModel.SenderID)?.AccountAmount < operationModel.TransferAmount)
                operationModel.OperationStatus = StatusOperationCode.Restricted;
        }
        else if (operationKind == OperationKind.Withdraw)
        {
            // SenderID is ID of user
            // ReceiverID is ID of bank
            if (!Banks.AsNoTracking().Any(x => x.ID == operationModel.ReceiverID) || !Users.Any(x => x.ID == operationModel.SenderID))
                operationModel.OperationStatus = StatusOperationCode.Error;
            if (BankAccounts.AsNoTracking().FirstOrDefault(x => x.UserID == operationModel.SenderID)?.BankAccountAmount < operationModel.TransferAmount)
                operationModel.OperationStatus = StatusOperationCode.Restricted;
        }

        if (!Banks.AsNoTracking().Any(x => x.ID == operationModel.BankID))
            operationModel.OperationStatus = StatusOperationCode.Error;

        return operationModel.OperationStatus;
    }
}