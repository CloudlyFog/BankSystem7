using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using BankSystem7.Services.Repositories;

namespace BankSystem7.Services;

public sealed class Logger : ILogger
{
    private readonly LoggerRepository _loggerRepository;
    private readonly ILogger _logger;

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
    }
    public Logger(ILogger logger)
    {
        _logger = logger;
        _loggerRepository = new LoggerRepository(logger.LoggerOptions);
    }

    public Logger(LoggerOptions options)
    {
        LoggerOptions = options;
        _loggerRepository = new LoggerRepository(options);
    }

    public Logger(LoggerRepository loggerRepository, LoggerOptions options)
    {
        LoggerOptions = options;
        _loggerRepository = loggerRepository;
    }

    public bool IsReused { get; set; }
    public LoggerOptions LoggerOptions { get; set; }

    public ExceptionModel Log(Report report)
    {
        if (!LoggerOptions.IsEnabled)
            return ExceptionModel.OperationRestricted;
        
        if (IsReused)
            return _logger.Log(report);

        if (_loggerRepository.Exist(x => x.Id == report.Id))
            _loggerRepository.Update(report);

        _loggerRepository.Create(report);

        return ExceptionModel.Successfully;
    }
}

public sealed class LoggerOptions
{
    public OperationServiceOptions? OperationServiceOptions { get; set; }
    public bool IsEnabled { get; set; }
}