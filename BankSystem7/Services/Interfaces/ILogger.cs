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
/// You have to implement this interface as part of repository, where logging doings is encapsulated.
/// For instance, implementation of method <see cref="Log"/> can be creating new report and logging it by interface <see cref="ILogger"/>
/// </summary>
public abstract class LoggerExecutor<TOperationType> where TOperationType : Enum 
{
    public void Log(ExceptionModel exceptionModel, string methodName, string className, TOperationType operationType, ICollection<GeneralReport<TOperationType>> reports)
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