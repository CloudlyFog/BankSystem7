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
public interface ILoggerDo<in TOperationType> where TOperationType : Enum 
{
    void Log(ExceptionModel exceptionModel, string methodName, string className, TOperationType operationType);
}