using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;

namespace BankSystem7.Services.Repositories;

public sealed class CardRepository : ApplicationContext, IRepository<Card>
{
    private CardContext _cardContext;
    private BankAccountRepository _bankAccountRepository;
    private bool _disposedValue;
    private readonly string _connection = @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=Test;Integrated Security=True;
            Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;
            Encrypt=False;TrustServerCertificate=False";

    public CardRepository()
    {
        _cardContext = new CardContext(BankServicesOptions.Connection);
        _bankAccountRepository = new BankAccountRepository(BankServicesOptions.Connection);
    }
    public CardRepository(BankAccountRepository bankAccountRepository)
    {
        _bankAccountRepository = bankAccountRepository;
        _cardContext = new CardContext(BankServicesOptions.Connection);
    }
    public CardRepository(BankAccountRepository bankAccountRepository, string connection)
    {
        _connection = connection;
        _bankAccountRepository = bankAccountRepository;
        _cardContext = new CardContext(connection);
    }
    public CardRepository(string connection)
    {
        _connection = connection;
        _cardContext = new CardContext(connection);
        _bankAccountRepository = new BankAccountRepository(connection);
    }

    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // Private implementation of Dispose pattern.
    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing)
        {
            _cardContext.Dispose();
            _bankAccountRepository.Dispose();
        }
        _cardContext = null;
        _bankAccountRepository = null;
        _disposedValue = true;
    }

    public async Task<ExceptionModel> Transfer(BankAccount? from, BankAccount? to, decimal transferAmount) 
        => await _bankAccountRepository.Transfer(from, to, transferAmount);

    public ExceptionModel Update(Card item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        Cards.Update(item);
        _cardContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    /// <summary>
    /// use this method only for creating new card. 
    /// if you'll try to use this method in designing card you'll get big bug which 'll force you program work wrong.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public ExceptionModel Create(Card item)
    {
        if (item.Exception == Warning.AgeRestricted)
            return ExceptionModel.OperationRestricted;
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        Cards.Add(item);
        _cardContext.SaveChanges();
        return ExceptionModel.Successfully;
    }

    public ExceptionModel Delete(Card item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;
        Cards.Remove(item);
        SaveChanges();
        return ExceptionModel.Successfully;
    }

    public bool Exist(Expression<Func<Card, bool>> predicate) => Cards.AsNoTracking().Any(predicate);
    public bool FitsConditions(Card item)
    {
        return item is not null && Exist(x => x.ID == item.ID);
    }

    public Card? Get(Expression<Func<Card, bool>> predicate) => Cards.AsNoTracking().FirstOrDefault(predicate);

    public IEnumerable<Card> All => Cards.AsNoTracking();
    public void ChangeTrackerCardContext() => ChangeTracker.Clear();

    ~CardRepository()
    {
        Dispose(false);
    }
}