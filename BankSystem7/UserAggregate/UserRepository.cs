using BankSystem7.ApplicationAggregate.Data;
using BankSystem7.ApplicationAggregate.Options;
using BankSystem7.BankAggregate;
using BankSystem7.BankAggregate.BankAccountAggregate;
using BankSystem7.BankAggregate.CardAggregate;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.UserAggregate;

public sealed class UserRepository(IBankAccountRepository bankAccountRepository,
                                   IBankRepository bankRepository,
                                   ICardRepository cardRepository,
                                   ApplicationContext context) : IUserRepository
{
    private bool _disposed;

    public IQueryable<User> Entities =>
        context.Users
            .Include(x => x.Card.BankAccount.Bank)
            .AsNoTracking();

    public ExceptionModel Create(User item)
    {
        if (item?.Card?.BankAccount?.Bank is null || item.Equals(User.Default))
            return ExceptionModel.OperationFailed;

        if (Exist(new(predicate: x => x.Id.Equals(item.Id)
                                                 || (x.Name.Equals(item.Name, StringComparison.Ordinal) && x.Email.Equals(item.Email, StringComparison.Ordinal)))))
            return ExceptionModel.OperationRestricted;

        context.UpdateTracker(item.Card.BankAccount.Bank, EntityState.Modified, () =>
        {
            item.Card.BankAccount.Bank.AccountAmount += bankRepository.CalculateBankAccountAmount(item.Card.Amount);
        }, context);

        context.Users.Add(item);

        context.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> CreateAsync(User item, CancellationToken cancellationToken = default)
    {
        if (item?.Card?.BankAccount?.Bank is null || item.Equals(User.Default))
            return ExceptionModel.OperationFailed;

        if (await ExistAsync(new(predicate: x => x.Id.Equals(item.Id)
                                                 || (x.Name.Equals(item.Name, StringComparison.Ordinal) && x.Email.Equals(item.Email, StringComparison.Ordinal))),
                                                 cancellationToken))
            return ExceptionModel.OperationRestricted;

        context.UpdateTracker(item.Card.BankAccount.Bank, EntityState.Modified, () =>
        {
            item.Card.BankAccount.Bank.AccountAmount += bankRepository.CalculateBankAccountAmount(item.Card.Amount);
        }, context);

        context.Users.Add(item);

        await context.SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    public ExceptionModel Update(User item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.OperationFailed;

        context.UpdateRange(item, item.Card.BankAccount);
        context.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> UpdateAsync(User item, CancellationToken cancellationToken = default)
    {
        if (!await FitsConditionsAsync(item, cancellationToken))
            return ExceptionModel.OperationFailed;

        context.UpdateRange(item, item.Card.BankAccount);
        await context.SaveChangesAsync(cancellationToken);
        return ExceptionModel.Ok;
    }

    public ExceptionModel Delete(User item)
    {
        if (!FitsConditions(item))
            return ExceptionModel.NotFitsConditions;

        context.RemoveRange(item, item.Card.BankAccount);
        context.UpdateTracker(item.Card.BankAccount.Bank, EntityState.Modified, () =>
        {
            item.Card.BankAccount.Bank.AccountAmount -=
                bankRepository.CalculateBankAccountAmount(item.Card.Amount);
        }, context);

        context.SaveChanges();
        return ExceptionModel.Ok;
    }

    public async Task<ExceptionModel> DeleteAsync(User item, CancellationToken cancellationToken = default)
    {
        if (!await FitsConditionsAsync(item, cancellationToken))
            return ExceptionModel.NotFitsConditions;

        context.RemoveRange(item, item.Card.BankAccount);
        context.UpdateTracker(item.Card.BankAccount.Bank, EntityState.Modified, () =>
        {
            item.Card.BankAccount.Bank.AccountAmount -=
                bankRepository.CalculateBankAccountAmount(item.Card.Amount);
        }, context);

        await context.SaveChangesAsync(cancellationToken);

        return ExceptionModel.Ok;
    }

    public bool Exist(EntityFindOptions<User> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return Entities.Any(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId));
    }

    public async Task<bool> ExistAsync(EntityFindOptions<User> entityFindOptions, CancellationToken cancellationToken = default)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return false;
        return await Entities.AnyAsync(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId), cancellationToken);
    }

    public bool FitsConditions(User? item)
    {
        return item?.Card?.BankAccount?.Bank is not null && Exist(new(item.Id));
    }

    public async Task<bool> FitsConditionsAsync(User? item, CancellationToken cancellationToken = default)
    {
        return item?.Card?.BankAccount?.Bank is not null && await ExistAsync(new(item.Id), cancellationToken);
    }

    public User Get(EntityFindOptions<User> entityFindOptions)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return User.Default;

        return Entities.FirstOrDefault(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId))
            ?? User.Default;
    }

    public async Task<User> GetAsync(EntityFindOptions<User> entityFindOptions, CancellationToken cancellationToken = default)
    {
        if (entityFindOptions.IsInvalidGetExpression())
            return User.Default;

        return await Entities.FirstOrDefaultAsync(entityFindOptions.Predicate ?? (x => x.Id == entityFindOptions.EntityId), cancellationToken)
            ?? User.Default;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        bankAccountRepository?.Dispose();
        bankRepository?.Dispose();
        cardRepository?.Dispose();
        context?.Dispose();

        _disposed = true;
    }
}