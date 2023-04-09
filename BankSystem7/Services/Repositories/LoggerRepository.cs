using System.Linq.Expressions;
using BankSystem7.Models;
using MongoDB.Driver;

namespace BankSystem7.Services.Repositories;

public sealed class LoggerRepository
{
    private readonly OperationService<Report> _operationService;

    public LoggerRepository()
    {
        LoggerOptions = new LoggerOptions
        {
            IsEnabled = true,
            OperationServiceOptions = new OperationServiceOptions
            {
                CollectionName = "Reports",
            },
        };
        _operationService = new OperationService<Report>(LoggerOptions.OperationServiceOptions ?? new OperationServiceOptions());
    }
    public LoggerRepository(LoggerOptions options)
    {
        _operationService = new OperationService<Report>(options?.OperationServiceOptions ?? new OperationServiceOptions());
        LoggerOptions = options;
    }
    public LoggerOptions LoggerOptions { get; }

    public ExceptionModel Create(Report? item)
    {
        if (item is null || Exist(x => x.Id == item.Id))
            return ExceptionModel.OperationFailed;
        _operationService.Collection.InsertOne(item);
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Update(Report item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        var filter = Builders<Report>.Filter
            .Eq(x => x.Id, item.Id);
        var update = Builders<Report>.Update
            .Set(x => x, item);
        _operationService.Collection.UpdateOne(filter, update);
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(Report item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        var filter = Builders<Report>.Filter
            .Eq(x => x.Id, item.Id);
        _operationService.Collection.DeleteOne(filter);
        return ExceptionModel.Successfully;
    }

    public IEnumerable<Report> All => _operationService.Collection.Find(_ => true).ToList();
    public Report Get(Expression<Func<Report, bool>> predicate)
    {
        var report = (GeneralReport<OperationType>)_operationService.Collection.Find(predicate).FirstOrDefault();
        report.ReportDate = DateTime.SpecifyKind(report.ReportDate, DateTimeKind.Local);
        return report;
    }

    public bool Exist(Expression<Func<Report, bool>> predicate)
    {
        return _operationService.Collection.Find(predicate).Any();
    }

    public bool FitsConditions(Report? item)
    {
        return item is not null && !Exist(x => x.Id == item.Id);
    }
}