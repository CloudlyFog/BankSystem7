using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Repositories;

public class CreditRepository : ApplicationContext, IRepository<Credit>
{
    private BankContext _bankContext;

    public CreditRepository()
    {
        _bankContext = BankServicesOptions.BankContext ?? new BankContext(BankServicesOptions.Connection);
    }

    public CreditRepository(string connection)
    {
        _bankContext = BankServicesOptions.BankContext ?? new BankContext(connection);
    }
    
    public ExceptionModel Create(Credit item)
    {
        if (item is null)
            return ExceptionModel.VariableIsNull;

        if (Exist(x => x.ID == item.ID))
            return ExceptionModel.OperationFailed;

        Credits.Add(item);
        SaveChanges();
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Update(Credit item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        Credits.Update(item);
        SaveChanges();
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(Credit item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        Credits.Remove(item);
        SaveChanges();
        return ExceptionModel.Successfully;
    }

    public IEnumerable<Credit> All => Credits.AsNoTracking();
    public Credit Get(Expression<Func<Credit, bool>> predicate) => Credits.AsNoTracking().FirstOrDefault(predicate);

    public bool Exist(Expression<Func<Credit, bool>> predicate) => Credits.AsNoTracking().Any(predicate);

    public bool FitsConditions(Credit item)
    {
        return item is not null && Exist(x => x.ID == item.ID);
    }
}