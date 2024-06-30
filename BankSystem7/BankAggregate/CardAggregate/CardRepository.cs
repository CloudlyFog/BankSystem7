using BankSystem7.ApplicationAggregate.Data;
using BankSystem7.ApplicationAggregate.Options;
using BankSystem7.BankAggregate.BankAccountAggregate;
using BankSystem7.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.BankAggregate.CardAggregate;

public sealed class CardRepository(IBankAccountRepository bankAccountRepository, ApplicationContext context) : ICardRepository
{
    private bool _disposedValue;

    public IQueryable<Card> Entities =>
        context.Cards
            .Include(x => x.User)
            .Include(x => x.BankAccount)
            .ThenInclude(x => x.Bank)
            .AsNoTracking() ?? Enumerable.Empty<Card>().AsQueryable();

    public IQueryable<Card> EntitiesTracking =>
        context.Cards
            .Include(x => x.User)
            .Include(x => x.BankAccount)
            .ThenInclude(x => x.Bank) ?? Enumerable.Empty<Card>().AsQueryable();

    /// <summary>
    /// use this method only for creating new card.
    /// if you'll try to use this method in designing card you'll get big bug which 'll force you program work wrong.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public ExceptionModel Create(Card item)
    {
        if (item.Exception == CardException.AgeRestricted)
            return ExceptionModel.OperationRestricted;

        if (item is not null && Exist(new(item.Id)))
            return ExceptionModel.OperationFailed;

        context.Cards.Add(item);
        context.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> CreateAsync(Card item, CancellationToken cancellationToken = default)
    {
        if (item.Exception == CardException.AgeRestricted)
            return ExceptionModel.OperationRestricted;

        if (item is not null && await ExistAsync(new(item.Id), cancellationToken))
            return ExceptionModel.OperationFailed;

        context.Cards.Add(item);
        await context.SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    public ExceptionModel Delete(Card item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        context.Cards.Remove(item);
        context.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> DeleteAsync(Card item, CancellationToken cancellationToken = default)
    {
        if (!await FitsConditionsAsync(item, cancellationToken))
            return ExceptionModel.OperationFailed;

        context.Cards.Remove(item);
        await context.SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    public ExceptionModel Update(Card item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        context.Cards.Update(item);
        context.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> UpdateAsync(Card item, CancellationToken cancellationToken = default)
    {
        if (!await FitsConditionsAsync(item, cancellationToken))
            return ExceptionModel.OperationFailed;

        context.Cards.Update(item);
        await context.SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    public Card Get(EntityFindOptions<Card> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return Card.Default;

        return Entities.FirstOrDefault(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId))
            ?? Card.Default;
    }

    public async Task<Card> GetAsync(EntityFindOptions<Card> entityFindOptions, CancellationToken cancellationToken = default)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return Card.Default;

        return await Entities.FirstOrDefaultAsync(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId), cancellationToken)
            ?? Card.Default;
    }

    public Card GetTracking(EntityFindOptions<Card> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return Card.Default;

        return EntitiesTracking.FirstOrDefault(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId))
            ?? Card.Default;
    }

    public bool Exist(EntityFindOptions<Card> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return Entities.Any(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId));
    }

    public async Task<bool> ExistAsync(EntityFindOptions<Card> entityFindOptions, CancellationToken cancellationToken = default)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return await Entities.AnyAsync(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId), cancellationToken);
    }

    public bool ExistTracking(EntityFindOptions<Card> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return EntitiesTracking.Any(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId));
    }

    public bool FitsConditions(Card? item)
    {
        return item is not null && Exist(new(item.Id));
    }

    public async Task<bool> FitsConditionsAsync(Card? item, CancellationToken cancellationToken = default)
    {
        return item is not null && await ExistAsync(new(item.Id), cancellationToken);
    }

    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose()
    {
        if (_disposedValue)
            return;

        bankAccountRepository?.Dispose();
        context?.Dispose();

        _disposedValue = true;
    }
}