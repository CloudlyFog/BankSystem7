using BankSystem7.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BankSystem7.ApplicationAggregate.Data;

public class GenericDbContext : DbContext
{
    private readonly ConfigurationOptions _configurationOptions;

    public GenericDbContext(ConfigurationOptions options)
    {
        DatabaseHandle(options);
        _configurationOptions = options;
    }

    public GenericDbContext(DbContextOptions dbContextOptions, ConfigurationOptions options) : base(dbContextOptions)
    {
        DatabaseHandle(options);
        _configurationOptions = options;
    }

    public override int SaveChanges()
    {
        var res = base.SaveChanges();
        ChangeTracker.Clear();
        return res;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var res = base.SaveChangesAsync(cancellationToken);
        ChangeTracker.Clear();
        return res;
    }

    public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
    {
        var entry = Entry(entity);
        if (entry.State == EntityState.Added)
            return entry;
        return base.Add(entity);
    }

    public override EntityEntry Add(object entity)
    {
        var entry = Entry(entity);
        if (entry.State == EntityState.Added)
            return entry;
        return base.Add(entity);
    }

    public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
    {
        var entry = Entry(entity);
        if (entry.State == EntityState.Modified)
            return entry;
        return base.Update(entity);
    }

    public override EntityEntry Update(object entity)
    {
        var entry = Entry(entity);
        if (entry.State == EntityState.Modified)
            return entry;
        return base.Update(entity);
    }

    public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
    {
        var entry = Entry(entity);
        if (entry.State == EntityState.Deleted)
            return entry;
        return base.Remove(entity);
    }

    public override EntityEntry Remove(object entity)
    {
        var entry = Entry(entity);
        if (entry.State == EntityState.Deleted)
            return entry;
        return base.Remove(entity);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (_configurationOptions.Database.ModelConfigurationAssembly is not null)
            modelBuilder.ApplyConfigurationsFromAssembly(_configurationOptions.Database.ModelConfigurationAssembly);
        else
            new ModelConfiguration().Invoke(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Method updates states of entity. You should use this method with method <see cref="AvoidChanges"/> for the best state tracking of entities
    /// </summary>
    /// <param name="item">entity whose state will be changed</param>
    /// <param name="state">future state of <see cref="item"/></param>
    /// <param name="action">action that will invoked after state of entity will be changed. Usually as action you should use method <see cref="AvoidChanges"/></param>
    /// <param name="context">context that will handle state changing</param>
    /// <typeparam name="TEntity">type of <see cref="item"/></typeparam>
    public void UpdateTracker<TEntity>(TEntity item, EntityState state, Action? action, DbContext context)
    {
        if (item is not null)
            context.Entry(item).State = state;
        action?.Invoke();
    }

    /// <summary>
    /// Method updates states of entities. You should use this method with method <see cref="AvoidChanges"/> for the best state tracking of entities
    /// </summary>
    /// <param name="items">array of entities whose state will be changed</param>
    /// <param name="state">future state of <see cref="items"/></param>
    /// <param name="action">action that will invoked after state of entity will be changed. Usually as action you should use method <see cref="AvoidChanges"/></param>
    /// <param name="context">context that will handle state changing</param>
    public void UpdateTrackerRange(object[] items, EntityState state, Action? action, DbContext context)
    {
        foreach (var item in items.Where(x => x is not null))
            context.Entry(item).State = state;
        action?.Invoke();
    }

    /// <summary>
    /// Method ensures that passed entities won't be changed during call method SaveChanges()
    /// </summary>
    /// <param name="entities">entities that shouldn't be changed</param>
    /// <param name="context">specifies what's context will handle operation</param>
    public void AvoidChanges(object[]? entities, DbContext context)
    {
        if (entities?.Length == 0)
            return;

        foreach (var entity in entities)
        {
            if (entity is not null)
                context.Entry(entity).State = EntityState.Unchanged;
        }
    }

    /// <summary>
    /// Handles creating and deleting database
    /// </summary>
    private void DatabaseHandle(ConfigurationOptions options)
    {
        if (options.Database.ConnectionConfiguration.EnsureDeleted)
            Database.EnsureDeleted();
        if (options.Database.ConnectionConfiguration.EnsureCreated)
            Database.EnsureCreated();
    }
}