using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace BankSystem7.Services.Repositories;

public sealed class OperationRepository<TUser, TCard, TBankAccount, TBank, TCredit> : LoggerExecutor<OperationType>, IRepository<Operation>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private OperationService<Operation> _operationService;

    public OperationRepository()
    {
        _operationService = new OperationService<Operation>();
    }

    public OperationRepository(OperationServiceOptions options)
    {
        _operationService = new OperationService<Operation>(options);
    }

    public void Dispose()
    {
        _operationService = null;
    }

    public ExceptionModel Create(Operation item)
    {
        if (item is null || Exist(x => x.ID == item.ID))
            return ExceptionModel.OperationFailed;
        _operationService.Collection.InsertOne(item);

        return ExceptionModel.Ok;
    }

    public ExceptionModel Update(Operation item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        var filter = Builders<Operation>.Filter
            .Eq(x => x.ID, item.ID);
        var update = Builders<Operation>.Update
            .Set(x => x, item);
        _operationService.Collection.UpdateOne(filter, update);

        return ExceptionModel.Ok;
    }

    public ExceptionModel Delete(Operation item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        var filter = Builders<Operation>.Filter
            .Eq(x => x.ID, item.ID);

        _operationService.Collection.DeleteOne(filter);
        return ExceptionModel.Ok;
    }

    public IQueryable<Operation> All => _operationService.Collection.Find(_ => true).ToList().AsQueryable();

    public Operation Get(Expression<Func<Operation, bool>> predicate)
    {
        return _operationService.Collection.Find(predicate).FirstOrDefault();
    }

    public bool Exist(Expression<Func<Operation, bool>> predicate)
    {
        return _operationService.Collection.Find(predicate).Any();
    }

    public bool FitsConditions(Operation? item)
    {
        if (item is null)
            return false;

        if (!Exist(x => x.ID == item.ID))
            return false;

        return true;
    }
}