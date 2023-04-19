using BankSystem7.Models;

namespace BankSystem7.Services.Interfaces;

public interface ILogger
{
    public bool IsReused { get; set; }
    public LoggerOptions LoggerOptions { get; set; }

    ExceptionModel Log(Report report);

    ExceptionModel Log(IEnumerable<Report> reports);
}

/// <summary>
/// Simple implementation of service for added reports to logger queue
/// </summary>
public abstract class LoggerExecutor<TOperationType> where TOperationType : Enum
{
    public virtual void Log(ExceptionModel exceptionModel, string methodName, string className, TOperationType operationType, ICollection<GeneralReport<TOperationType>> reports)
    {
        var report = new GeneralReport<TOperationType>
        {
            MethodName = methodName,
            ClassName = className,
            OperationType = operationType,
            ExceptionModel = exceptionModel,
        };
        reports.Add(report);
    }
}