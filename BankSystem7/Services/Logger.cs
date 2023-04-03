using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using MongoDB.Driver;

namespace BankSystem7.Services;

public class Logger<T> : ILogger<T> where T : Report
{
    private readonly OperationService<T> _operationService;
    private readonly ILogger<T> _logger;

    public Logger()
    {
        _operationService = new OperationService<T>(new OperationServiceOptions
        {
            CollectionName = "Reports",
        });
    }
    public Logger(ILogger<T> logger)
    {
        _logger = logger;
    }

    public Logger(OperationServiceOptions options)
    {
        _operationService = new OperationService<T>(options);
    }

    public bool IsReused { get; set; }
    
    public async Task<ExceptionModel> Log(Report report)
    {
        if (IsReused)
            return await _logger.Log(report);

        if (Exists(report))
            return ExceptionModel.OperationFailed;

        
        await _operationService.Collection.InsertOneAsync((T)report);

        return ExceptionModel.Successfully;
    }

    private Report? Get(Report report)
    {
        return _operationService.Collection.Find(x => x.Id == report.Id).FirstOrDefault();
    }

    private bool Exists(Report report)
    {
        return Get(report) is not null;
    }
}