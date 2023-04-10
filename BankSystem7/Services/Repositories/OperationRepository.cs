using System.Linq.Expressions;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using MongoDB.Driver;

namespace BankSystem7.Services.Repositories;

public class OperationRepository : LoggerExecutor<OperationType>, IRepository<Operation>
{
    private List<GeneralReport<OperationType>> _reports = new();
    private OperationService<Operation> _operationService;
    private ILogger _logger;

    public OperationRepository()
    {
        _operationService = new OperationService<Operation>();
        _logger = new Logger();
    }
    public OperationRepository(OperationServiceOptions options)
    {
        _operationService = new OperationService<Operation>(options);
        _logger = new Logger(new LoggerOptions
        {
            OperationServiceOptions = options,
        });
    }
    public OperationRepository(ILogger logger, OperationServiceOptions options)
    {
        _operationService = new OperationService<Operation>(options);
        _logger = logger;
    }
    public OperationRepository(LoggerRepository loggerRepository, LoggerOptions options)
    {
        _operationService = new OperationService<Operation>(options.OperationServiceOptions);
        _logger = new Logger(loggerRepository, options);
    }
    public void Dispose()
    {
        _logger.Log(_reports);
        _operationService = null;
        _logger = null;
        _reports = null;
    }

    public ExceptionModel Create(Operation item)
    {
        if (item is null || Exist(x => x.ID == item.ID))
        {
            Log(ExceptionModel.OperationFailed, nameof(Create), nameof(OperationRepository), OperationType.Create, _reports);
            return ExceptionModel.OperationFailed;
        }
        _operationService.Collection.InsertOne(item);
        
        Log(ExceptionModel.Successfully, nameof(Create), nameof(OperationRepository), OperationType.Create, _reports);
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Update(Operation item)
    {
        if (!FitsConditions(item))
        {
            Log(ExceptionModel.OperationFailed, nameof(Update), nameof(OperationRepository), OperationType.Update, _reports);
            return ExceptionModel.OperationFailed;
        }

        var filter = Builders<Operation>.Filter
            .Eq(x => x.ID, item.ID);
        var update = Builders<Operation>.Update
            .Set(x => x, item);
        _operationService.Collection.UpdateOne(filter, update);
        
        Log(ExceptionModel.Successfully, nameof(Update), nameof(OperationRepository), OperationType.Update, _reports);
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(Operation item)
    {
        if (!FitsConditions(item))
        {
            Log(ExceptionModel.OperationFailed, nameof(Delete), nameof(OperationRepository), OperationType.Delete, _reports);
            return ExceptionModel.OperationFailed;
        }
        var filter = Builders<Operation>.Filter
            .Eq(x => x.ID, item.ID);
        
        Log(ExceptionModel.Successfully, nameof(Delete), nameof(OperationRepository), OperationType.Delete, _reports);
        _operationService.Collection.DeleteOne(filter);
        return ExceptionModel.Successfully;
    }

    public IEnumerable<Operation> All => _operationService.Collection.Find(_ => true).ToList();
    public Operation Get(Expression<Func<Operation, bool>> predicate)
    {
        return _operationService.Collection.Find(predicate).FirstOrDefault();
    }

    public bool Exist(Expression<Func<Operation, bool>> predicate)
    {
        Log(ExceptionModel.Successfully, nameof(Exist), nameof(OperationRepository), OperationType.Exist, _reports);
        return _operationService.Collection.Find(predicate).Any();
    }

    public bool FitsConditions(Operation? item)
    {
        if (item is null)
        {
            Log(ExceptionModel.VariableIsNull, nameof(FitsConditions), nameof(OperationRepository), OperationType.FitsConditions, _reports);
            return false;
        }

        if (!Exist(x => x.ID == item.ID))
        {
            Log(ExceptionModel.OperationNotExist, nameof(FitsConditions), nameof(OperationRepository), OperationType.FitsConditions, _reports);
            return false;
        }

        return true;
    }
}