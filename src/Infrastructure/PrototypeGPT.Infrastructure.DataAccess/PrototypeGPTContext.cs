namespace PrototypeGPT.Infrastructure.DataAccess;

using PrototypeGPT.Application.DataAccess;
using PrototypeGPT.Domain;
using PrototypeGPT.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// This class implement the interface <see cref="IDbContext"/>
/// </summary>
public class PrototypeGPTContext : IdentityDbContext<User, Role, Guid>, IDbContext
{
    /// <summary>
    /// A transaction Object
    /// </summary>
    private IDbContextTransaction transaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrototypeGPTContext" /> class
    /// </summary>
    /// <param name="options">DB Context options</param>
    public PrototypeGPTContext(DbContextOptions options)
    : base(options)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        RunSeed(modelBuilder);
    }

    /// <summary>
    /// Run seeding
    /// </summary>
    private static void RunSeed(ModelBuilder modelBuilder)
    {
        // IdentitySeeding.Run(modelBuilder);
    }

    /// <inheritdoc/>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changes = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var item in changes)
        {
            item.Property(p => p.UpdatedAt).CurrentValue = DateTime.UtcNow;

            if (item.State == EntityState.Added)
            {
                item.Property(p => p.CreatedAt).CurrentValue = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override int SaveChanges()
    {
        var changes = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var item in changes)
        {
            item.Property(p => p.UpdatedAt).CurrentValue = DateTime.UtcNow;
            if (item.State == EntityState.Added)
            {
                item.Property(p => p.CreatedAt).CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChanges();
    }

    #region IDbContext

    /// <inheritdoc/>
    public void BeginTransaction(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (transaction != null)
        {
            var dbTransaction = transaction.GetDbTransaction();
            try
            {
                if (dbTransaction != null && dbTransaction?.Connection != null && dbTransaction?.Connection?.State == System.Data.ConnectionState.Open)
                {
                    return;
                }
            }
            catch (Exception)
            {

            }
        }

        transaction = Database.BeginTransaction();
    }

    /// <inheritdoc/>
    public int Commit(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            BeginTransaction(cancellationToken);
            var saveChanges = SaveChanges();
            EndTransaction(cancellationToken);

            return saveChanges;
        }
        catch (Exception)
        {
            Rollback(cancellationToken);
            throw;
        }
        finally
        {
            // base.Dispose();
        }
    }

    /// <inheritdoc/>
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            BeginTransaction(cancellationToken);
            var saveChangesAsync = await SaveChangesAsync(cancellationToken);
            EndTransaction(cancellationToken);

            return saveChangesAsync;
        }
        catch (Exception)
        {
            Rollback(cancellationToken);
            throw;
        }
        finally
        {
            // base.Dispose();
        }
    }

    /// <inheritdoc/>
    public void Rollback(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (transaction != null && transaction.GetDbTransaction().Connection != null)
        {
            transaction.Rollback();
        }
    }

    /// <inheritdoc/>
    private void EndTransaction(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        transaction.Commit();
    }

    /// <inheritdoc/>
    public void SetAsAdded<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        UpdateEntityState<TEntity>(entity, EntityState.Added, cancellationToken);
    }

    /// <inheritdoc/>
    public void SetAsAdded<TEntity>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        entities.ForEach(entity => SetAsAdded<TEntity>(entity, cancellationToken));
    }

    /// <inheritdoc/>
    public void SetAsModified<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        UpdateEntityState<TEntity>(entity, EntityState.Modified, cancellationToken);
    }

    /// <inheritdoc/>
    public void SetAsModified<TEntity>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        entities.ForEach(entity => SetAsModified<TEntity>(entity, cancellationToken));
    }

    /// <inheritdoc/>
    public void SetAsDeleted<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        UpdateEntityState<TEntity>(entity, EntityState.Deleted, cancellationToken);
    }

    /// <inheritdoc/>
    public void SetAsDeleted<TEntity>(List<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        entities.ForEach(entity => SetAsDeleted<TEntity>(entity, cancellationToken));
    }

    /// <inheritdoc/>
    public Task<TEntity> FindAsync<TEntity>(Guid id, CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Set<TEntity>().FindAsync(id, cancellationToken).AsTask();
    }

    /// <inheritdoc/>
    public Task<TEntity> FindByCriteriaAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Set<TEntity>().Local.AsQueryable().FirstOrDefaultAsync(predicate, cancellationToken) ?? FirstOrDefaultAsync<TEntity>(predicate, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<TEntity> FirstOrDefaultAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Set<TEntity>().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<TEntity>> ToListAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Set<TEntity>().ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<TEntity>> ToListByCriteriaAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Set<TEntity>().Where(predicate).ToListAsync<TEntity>(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Set<TEntity>().AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc/>
    public IQueryable<TEntity> ToQueryable<TEntity>() where TEntity : class
    {
        return Set<TEntity>().AsQueryable();
    }

    /// <inheritdoc/>
    public IQueryable<TEntity> ToQueryableByCriteria<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        return Set<TEntity>().Where(predicate).AsQueryable();
    }

    /// <summary>
    /// Update entity state
    /// </summary>
    private void UpdateEntityState<TEntity>(TEntity entity, EntityState entityState, CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        var entityEntry = GetDbEntityEntrySafely<TEntity>(entity, cancellationToken);
        if (entityEntry.State == EntityState.Unchanged)
        {
            entityEntry.State = entityState;
        }
    }

    /// <summary>
    /// Attach entity
    /// </summary>
    private EntityEntry GetDbEntityEntrySafely<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        cancellationToken.ThrowIfCancellationRequested();

        var entityEntry = Entry<TEntity>(entity);
        if (entityEntry.State == EntityState.Detached)
        {
            Set<TEntity>().Attach(entity);
        }

        return entityEntry;
    }

    #endregion IDbContext
}
