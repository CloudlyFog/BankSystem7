using BankSystem7.Models;

namespace BankSystem7.Services.Interfaces;

public interface ILogger<T> where T : Report
{
    public bool IsReused { get; set; }
    Task<ExceptionModel> Log(Report report);
}