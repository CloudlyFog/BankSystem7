using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using BankSystem7.ApplicationAggregate.Interfaces.Readers;
using BankSystem7.ApplicationAggregate.Options;

namespace BankSystem7.ApplicationAggregate.Entities;

public abstract class Entity : IEqualityComparer<Entity>, IEquatable<Entity>
{
    protected Entity(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; private init; }

    public bool Equals(Entity? other)
    {
        if (other is null)
            return false;

        if (other.GetType() != GetType())
            return false;

        return other.Id == Id;
    }

    public bool Equals(Entity? x, Entity? y)
    {
        if (x is null || y is null)
            return false;

        return x.Id == y.Id;
    }

    public override bool Equals(object obj)
    {
        return Equals(this, obj as Entity);
    }

    public int GetHashCode([DisallowNull] Entity obj)
    {
        return obj.Id.GetHashCode() * 12;
    }

    public bool IsEmpty()
    {
        return Id == Guid.Empty;
    }

    public bool IsExist<TReaderEntity>(IReader<TReaderEntity> reader, Expression<Func<TReaderEntity, bool>> func)
        where TReaderEntity : Entity
    {
        if (reader is null)
            return true;
        return reader.Exist(new EntityFindOptions<TReaderEntity>(predicate: func));
    }

    public bool IsValid<TReaderEntity>(IReader<TReaderEntity> reader)
        where TReaderEntity : Entity
    {
        return IsValid(reader, x => x.Id == Id);
    }

    public bool IsValid<TReaderEntity>(IReader<TReaderEntity> reader, Expression<Func<TReaderEntity, bool>> func)
        where TReaderEntity : Entity
    {
        return !IsEmpty() && IsExist(reader, func);
    }
}