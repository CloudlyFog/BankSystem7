using BankSystem7.Models;
using BankSystem7.Services;
using BankSystem7.Services.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MongoDB.Driver;
using System.Buffers;

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
        if (operation.OperationStatus is not StatusOperationCode.Ok or StatusOperationCode.Successfully)
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
        if (operationModel is null || !Banks.AsNoTracking().Any(x => x.ID == operationModel.BankID))
            return StatusOperationCode.Error;

        if (operationKind == OperationKind.Accrual)
        {
            // Check if the receiver ID exists in the Users table without tracking changes
            if (!Users.AsNoTracking().Select(x => x.ID).Any(x => x == operationModel.ReceiverID))
                return StatusOperationCode.Error;

            // Check if the sender ID exists in the Banks table and if the account amount is less than the transfer amount
            if (Banks.AsNoTracking().FirstOrDefault(x => x.ID == operationModel.SenderID)?.AccountAmount < operationModel.TransferAmount)
                return StatusOperationCode.Restricted;
        }
        else if (operationKind == OperationKind.Withdraw)
        {
            // Check if the SenderID of the operationModel exists in the Users table
            if (!Users.AsNoTracking().Select(x => x.ID).Any(x => x == operationModel.SenderID)) 
                return StatusOperationCode.Error;

            // Check if the SenderID of the operationModel has enough funds in their bank account
            if (BankAccounts.AsNoTracking().FirstOrDefault(x => x.UserID == operationModel.SenderID)?.BankAccountAmount < operationModel.TransferAmount) 
                return StatusOperationCode.Restricted;
        }

        return StatusOperationCode.Ok;
    }
}