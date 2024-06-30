using BankSystem7.ApplicationAggregate.Options;
using BankSystem7.UserAggregate;
using MongoDB.Driver;

namespace BankSystem7.BankAggregate.OperationAggregate;

public sealed class OperationRepository : IOperationRepository
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

    public IQueryable<Operation> Entities => _operationService.Collection.Find(_ => true).ToList().AsQueryable();

    public void Dispose()
    {
        _operationService = null;
    }

    public ExceptionModel Create(Operation item)
    {
        if (item is null || Exist(new(item.Id)))
            return ExceptionModel.OperationFailed;
        _operationService.Collection.InsertOne(item);

        return ExceptionModel.Ok;
    }

    public ExceptionModel Update(Operation item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        var filter = Builders<Operation>.Filter
            .Eq(x => x.Id, item.Id);
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
            .Eq(x => x.Id, item.Id);

        _operationService.Collection.DeleteOne(filter);
        return ExceptionModel.Ok;
    }

    public Operation Get(EntityFindOptions<Operation> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return Operation.Default;
        return _operationService.Collection
            .Find(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId))
            .FirstOrDefault();
    }

    public bool Exist(EntityFindOptions<Operation> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return _operationService.Collection
            .Find(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId))
            .Any();
    }

    public bool FitsConditions(Operation? item)
    {
        if (item is null)
            return false;

        if (!Exist(new(item.Id)))
            return false;

        return true;
    }
}