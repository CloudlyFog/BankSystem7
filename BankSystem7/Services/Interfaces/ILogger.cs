using BankSystem7.Models;

namespace BankSystem7.Services.Interfaces;

public interface ILogger
{
    public bool IsReused { get; set; }
    public LoggerOptions LoggerOptions { get; set; }
    ExceptionModel Log(Report report);
}