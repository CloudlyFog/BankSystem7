﻿using BankSystem7.Models;
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
    private List<GeneralReport<OperationType>> _reports = new();
    private OperationService<Operation> _operationService;
    private ILogger _logger;

    public OperationRepository()
    {
        _operationService = new OperationService<Operation>();
        _logger = new Logger<TUser, TCard, TBankAccount, TBank, TCredit>();
    }

    public OperationRepository(OperationServiceOptions options)
    {
        _operationService = new OperationService<Operation>(options);
        _logger = new Logger<TUser, TCard, TBankAccount, TBank, TCredit>(new LoggerOptions
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
        _logger = new Logger<TUser, TCard, TBankAccount, TBank, TCredit>(loggerRepository, options);
    }

    public void Dispose()
    {
        _logger?.Log(_reports);
        _operationService = null;
        _logger = null;
        _reports = null;
    }

    public ExceptionModel Create(Operation item)
    {
        if (item is null || Exist(x => x.ID == item.ID))
            return ExceptionModel.OperationFailed;
        _operationService.Collection.InsertOne(item);

        return ExceptionModel.Successfully;
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

        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(Operation item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        var filter = Builders<Operation>.Filter
            .Eq(x => x.ID, item.ID);

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