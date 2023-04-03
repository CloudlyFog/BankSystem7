using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using MongoDB.Driver;
using SharpCompress.Readers;

namespace BankSystem7.Services;

public class Logger<T> : ILogger<T> where T : Report
{
    private readonly OperationService<T> _operationService;
    private readonly ILogger<T> _logger;

    public Logger()
    {
        LoggerOptions = new LoggerOptions
        {
            IsEnabled = true,
            OperationServiceOptions = new OperationServiceOptions
            {
                CollectionName = "Reports",
            },
        };
        _operationService = new OperationService<T>(LoggerOptions.OperationServiceOptions);
    }
    public Logger(ILogger<T> logger)
    {
        _logger = logger;
    }

    public Logger(LoggerOptions options)
    {
        _operationService = new OperationService<T>(options.OperationServiceOptions);
        LoggerOptions = options;
    }
    
    public LoggerOptions LoggerOptions { get; }

    public bool IsReused { get; set; }
    
    public async Task<ExceptionModel> Log(Report report)
    {
        if (!LoggerOptions.IsEnabled)
            return ExceptionModel.OperationRestricted;
        
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

public sealed class LoggerOptions
{
    public OperationServiceOptions? OperationServiceOptions { get; set; }
    public bool IsEnabled { get; set; }
}
public class Logger
{
    public static bool IsEnabled { get; set; }
}