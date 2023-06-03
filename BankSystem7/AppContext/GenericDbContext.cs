using BankSystem7.Services.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BankSystem7.AppContext;

public class GenericDbContext : DbContext
{
    public GenericDbContext()
    {
        ModelCreatingOptions.ModelConfiguration = new ModelConfiguration();
        DatabaseHandle();
    }

    public GenericDbContext(string connection)
    {
        ServicesSettings.SetConnection(connection);
        ModelCreatingOptions.ModelConfiguration = new ModelConfiguration();
        DatabaseHandle();
    }

    public GenericDbContext(string connection, bool useOwnAccessConfiguration = false)
    {
        ServicesSettings.SetConnection(connection);
        ModelCreatingOptions.ModelConfiguration = new ModelConfiguration();
        ServicesSettings.InitializeAccess = useOwnAccessConfiguration;
        DatabaseHandle();
    }

    public GenericDbContext(ModelConfiguration? modelConfiguration)
    {
        ModelCreatingOptions.ModelConfigurations?.Add(modelConfiguration ?? new ModelConfiguration());
        ServicesSettings.InitializeAccess =
            modelConfiguration.InitializeAccess;
        if (ModelCreatingOptions.LastModelConfiguration)
            DatabaseHandle();
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        SetDatabaseManagementSystemType(optionsBuilder, ServicesSettings.DatabaseManagementSystemType);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new ModelConfiguration().Invoke(modelBuilder, ModelCreatingOptions.ModelConfigurations);

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Method updates states of entity. You should use this method with method <see cref="AvoidChanges"/> for the best state tracking of entities
    /// </summary>
    /// <param name="item">entity whose state will be changed</param>
    /// <param name="state">future state of <see cref="item"/></param>
    /// <param name="action">action that will invoked after state of entity will be changed. Usually as action you should use method <see cref="AvoidChanges"/></param>
    /// <param name="context">context that will handle state changing</param>
    /// <typeparam name="T">type of <see cref="item"/></typeparam>
    public void UpdateTracker<T>(T item, EntityState state, Action? action, DbContext context)
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
    private void DatabaseHandle()
    {
        if (!ServicesSettings.InitializeAccess || ServicesSettings.Ensured)
            return;
        if (ServicesSettings.EnsureDeleted)
            Database.EnsureDeleted();
        if (ServicesSettings.EnsureCreated)
            Database.EnsureCreated();
        ServicesSettings.Ensured = true;
        ServicesSettings.InitializeAccess = false;
    }

    /// <summary>
    /// This method sets the database management system type for the given DbContextOptionsBuilder object.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="dbmsType">Type of the DBMS.</param>
    /// <exception cref="ArgumentOutOfRangeException">dbmsType - null</exception>
    private void SetDatabaseManagementSystemType(DbContextOptionsBuilder optionsBuilder, DatabaseManagementSystemType dbmsType)
    {
        switch (dbmsType)
        {
            case DatabaseManagementSystemType.MicrosoftSqlServer:
                optionsBuilder.UseSqlServer(ServicesSettings.Connection, o => o.CommandTimeout(20));
                break;
            case DatabaseManagementSystemType.PostgreSql:
                optionsBuilder.UseNpgsql(ServicesSettings.Connection, o => o.CommandTimeout(20));
                break;
            case DatabaseManagementSystemType.MySql:
                optionsBuilder.UseMySQL(ServicesSettings.Connection, o => o.CommandTimeout(20));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dbmsType), dbmsType, null);
        }
    }
}