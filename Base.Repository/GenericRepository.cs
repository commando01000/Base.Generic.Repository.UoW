using Base.Repository;
using Base.Repository.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Repository.Layer.Specification;
using System.Data;
using System.Linq.Expressions;
using System.Threading;

namespace Repository.Layer
{
    public class GenericRepository<TEntity, TKey, TContext> : IGenericRepository<TEntity, TKey>
        where TEntity : class
        where TContext : DbContext
    {
        protected readonly TContext _context;
        private readonly ILogger<GenericRepository<TEntity, TKey, TContext>> _logger;

        public GenericRepository(TContext context, ILogger<GenericRepository<TEntity, TKey, TContext>> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ---------------------------
        // Create / Update / Delete
        // ---------------------------

        public async Task<Guid> Create(TEntity entity)
        {
            if (entity == null)
            {
                _logger.LogWarning("Attempted to create a null entity: {EntityType}", typeof(TEntity).Name);
                return Guid.Empty;
            }

            try
            {
                await _context.Set<TEntity>().AddAsync(entity);

                // Assume TEntity has a Guid Id property
                var idProperty = entity.GetType().GetProperty("Id");
                if (idProperty == null || idProperty.PropertyType != typeof(Guid))
                {
                    _logger.LogError("Entity {EntityType} does not have a Guid Id property", typeof(TEntity).Name);
                    return Guid.Empty;
                }

                var id = (Guid)idProperty.GetValue(entity);
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity: {EntityType}", typeof(TEntity).Name);
                return Guid.Empty;
            }
        }

        public async Task<bool> Update(TEntity entity)
        {
            if (entity == null)
            {
                _logger.LogWarning("Attempted to update a null entity: {EntityType}", typeof(TEntity).Name);
                return false;
            }

            try
            {
                _context.Set<TEntity>().Update(entity);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity: {EntityType}", typeof(TEntity).Name);
                return false;
            }
        }

        public async Task BulkInsertAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null || !entities.Any()) return;
            await _context.Set<TEntity>().AddRangeAsync(entities);
        }

        public async Task<bool> Delete(TEntity entity)
        {
            if (entity == null)
            {
                _logger.LogWarning("Attempted to delete a null entity: {EntityType}", typeof(TEntity).Name);
                return false;
            }

            try
            {
                _context.Set<TEntity>().Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity: {EntityType}", typeof(TEntity).Name);
                return false;
            }
        }

        public async Task<bool> SoftDeleteAsync(TKey id)
        {
            var entity = await Get(id);
            if (entity == null) return false;

            var prop = typeof(TEntity).GetProperty("IsDeleted");
            if (prop == null || prop.PropertyType != typeof(bool)) return false;

            prop.SetValue(entity, true);
            _context.Set<TEntity>().Update(entity);
            return true;
        }

        // ---------------------------
        // Basic Gets
        // ---------------------------

        public async Task<TEntity> Get(Expression<Func<TEntity, bool>> spec)
        {
            try { return await _context.Set<TEntity>().FirstOrDefaultAsync(spec); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entity by specification: {EntityType}", typeof(TEntity).Name);
                return null;
            }
        }

        public async IAsyncEnumerable<TEntity> GetAll()
        {
            await foreach (var entity in _context.Set<TEntity>().AsAsyncEnumerable())
                yield return entity;
        }

        public async Task<List<TEntity>> GetAllAsNoTracking()
        {
            try { return await _context.Set<TEntity>().AsNoTracking().ToListAsync(); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all entities (No Tracking): {EntityType}", typeof(TEntity).Name);
                return new List<TEntity>();
            }
        }

        public async Task<List<TEntity>> GetAllAsNoTracking(Expression<Func<TEntity, bool>> spec)
        {
            try { return await _context.Set<TEntity>().AsNoTracking().Where(spec).ToListAsync(); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entities (No Tracking) by condition: {EntityType}", typeof(TEntity).Name);
                return new List<TEntity>();
            }
        }

        public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> spec)
        {
            try { return await _context.Set<TEntity>().Where(spec).ToListAsync(); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entities by condition: {EntityType}", typeof(TEntity).Name);
                return new List<TEntity>();
            }
        }

        public async Task<IEnumerable<TEntity>> GetAllWithSpecs(ISpecification<TEntity> specs)
        {
            try
            {
                return await SpecificationEvaluator<TEntity>
                    .GetQuery(_context.Set<TEntity>().AsQueryable(), specs)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entities with specifications: {EntityType}", typeof(TEntity).Name);
                return new List<TEntity>();
            }
        }

        public async Task<PaginatedResult<TEntity>> GetAllPaginatedAsync(int pageIndex, int pageSize)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.Set<TEntity>();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<TEntity>(totalCount, pageIndex, pageSize, items);
        }

        public async Task<List<TEntity>> GetAllWithIncludesAsync(params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (includes != null && includes.Any())
                foreach (var include in includes) query = query.Include(include);

            return await query.ToListAsync();
        }

        public async Task<PaginatedResult<TEntity>> GetAllWithIncludesPaginatedAsync(int pageIndex, int pageSize, params Expression<Func<TEntity, object>>[] includes)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (includes != null && includes.Any())
                foreach (var include in includes) query = query.Include(include);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<TEntity>(totalCount, pageIndex, pageSize, items);
        }

        public async Task<TEntity> GetByIdWithIncludesAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (includes != null && includes.Any())
                foreach (var include in includes) query = query.Include(include);

            return await query.FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id").Equals(id));
        }

        public async Task<PaginatedResult<TEntity>> GetAllWithIncludesPaginatedAsNoTrackingAsync(int pageIndex, int pageSize, params Expression<Func<TEntity, object>>[] includes)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking();
            if (includes != null && includes.Any())
                foreach (var include in includes) query = query.Include(include);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<TEntity>(totalCount, pageIndex, pageSize, items);
        }

        public async Task<TEntity> Get(TKey id)
        {
            try { return await _context.Set<TEntity>().FindAsync(id); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entity by ID: {EntityType}, ID: {EntityId}", typeof(TEntity).Name, id);
                return null;
            }
        }

        public async Task<TEntity> GetWithSpecs(ISpecification<TEntity> specs)
        {
            try
            {
                return await SpecificationEvaluator<TEntity>
                    .GetQuery(_context.Set<TEntity>().AsQueryable(), specs)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entity with specifications: {EntityType}", typeof(TEntity).Name);
                return null;
            }
        }

        public async Task<int> CountSpecsAsync(ISpecification<TEntity> specs)
            => await SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>(), specs).CountAsync();

        public async Task<List<TEntity>> GetAllAsNoTracking(ISpecification<TEntity> spec)
            => await SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsNoTracking(), spec).ToListAsync();

        public async Task<PaginatedResult<TEntity>> GetAllPaginatedAsNoTrackingAsync(int pageIndex, int pageSize)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.Set<TEntity>().AsNoTracking();

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<TEntity>(totalCount, pageIndex, pageSize, items);
        }

        public async Task<PaginatedResult<TEntity>> GetAllWithSpecsPaginatedAsync(ISpecification<TEntity> spec, int pageIndex, int pageSize)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsQueryable(), spec);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<TEntity>(totalCount, pageIndex, pageSize, items);
        }

        public async Task<PaginatedResult<TEntity>> GetAllPaginatedAsNoTrackingAsync(
            int pageIndex,
            int pageSize,
            params Expression<Func<TEntity, bool>>[] predicates)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking();

            if (predicates != null && predicates.Length > 0)
                foreach (var predicate in predicates)
                    if (predicate != null) query = query.Where(predicate);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<TEntity>(totalCount, pageIndex, pageSize, items);
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
            => await _context.Set<TEntity>().AnyAsync(predicate);

        public async Task<int> CountAsync()
            => await _context.Set<TEntity>().CountAsync();

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
            => await _context.Set<TEntity>().CountAsync(predicate);

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _context.SaveChangesAsync(ct);

        // ---------------------------
        // Additional Helpers (new)
        // ---------------------------

        public async Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (asNoTracking) query = query.AsNoTracking();
            if (includes != null) foreach (var include in includes) query = query.Include(include);
            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<TEntity?> SingleOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (asNoTracking) query = query.AsNoTracking();
            if (includes != null) foreach (var include in includes) query = query.Include(include);
            return await query.SingleOrDefaultAsync(predicate);
        }

        public async Task<List<TEntity>> ListAsync(
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (asNoTracking) query = query.AsNoTracking();
            if (includes != null) foreach (var include in includes) query = query.Include(include);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) query = orderBy(query);
            return await query.ToListAsync();
        }

        public async Task<List<TOut>> ListSelectAsync<TOut>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TOut>> selector,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (asNoTracking) query = query.AsNoTracking();
            if (includes != null) foreach (var include in includes) query = query.Include(include);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) query = orderBy(query);
            return await query.Select(selector).ToListAsync();
        }

        public async Task<PaginatedResult<TOut>> GetPageSelectAsync<TOut>(
        int pageIndex,
        int pageSize,
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TOut>> selector,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
        bool asNoTracking = true,
        params Expression<Func<TEntity, object>>[] includes)
        where TOut : class  // <-- add this
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (asNoTracking) query = query.AsNoTracking();
            if (includes != null) foreach (var include in includes) query = query.Include(include);
            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) query = orderBy(query);

            var total = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToListAsync();

            return new PaginatedResult<TOut>(total, pageIndex, pageSize, items);
        }


        public Task<int> DeleteWhereAsync(Expression<Func<TEntity, bool>> predicate)
            => _context.Set<TEntity>().Where(predicate).ExecuteDeleteAsync();

        // after:
        public Task<int> UpdateWhereAsync(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setProps)
            => _context.Set<TEntity>().Where(predicate).ExecuteUpdateAsync(setProps);

        public Task<int> SoftDeleteWhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return _context.Set<TEntity>()
                .Where(predicate)
                .ExecuteUpdateAsync(s => s.SetProperty(e => EF.Property<bool>(e, "IsDeleted"), _ => true));
        }

        public Task<bool> ExistsByIdAsync(TKey id)
            => _context.Set<TEntity>().AnyAsync(e => EF.Property<TKey>(e, "Id")!.Equals(id));

        public async Task<List<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids, bool asNoTracking = true)
        {
            if (ids == null) return new List<TEntity>();
            IQueryable<TEntity> q = _context.Set<TEntity>();
            if (asNoTracking) q = q.AsNoTracking();

            // Note: EF will translate Contains on EF.Property
            return await q.Where(e => ids.Contains(EF.Property<TKey>(e, "Id"))).ToListAsync();
        }

        public async Task<List<TEntity>> GetSliceAsync<TOrderKey>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TOrderKey>> orderBy,
            TOrderKey afterKey,
            int take,
            bool ascending = true,
            bool asNoTracking = true)
        {
            IQueryable<TEntity> q = _context.Set<TEntity>();
            if (asNoTracking) q = q.AsNoTracking();
            if (predicate != null) q = q.Where(predicate);

            // Seek pagination: order + key comparison
            if (ascending)
            {
                q = q.Where(e => Comparer<TOrderKey>.Default.Compare(orderBy.Compile()(e), afterKey) > 0)
                     .OrderBy(orderBy);
            }
            else
            {
                q = q.Where(e => Comparer<TOrderKey>.Default.Compare(orderBy.Compile()(e), afterKey) < 0)
                     .OrderByDescending(orderBy);
            }

            return await q.Take(take).ToListAsync();
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action, IsolationLevel? isolation = null)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = isolation.HasValue
                    ? await _context.Database.BeginTransactionAsync(isolation.Value)
                    : await _context.Database.BeginTransactionAsync();

                await action();
                await tx.CommitAsync();
            });
        }

        public IQueryable<TEntity> Query(bool asNoTracking = true, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> q = _context.Set<TEntity>();
            if (asNoTracking) q = q.AsNoTracking();
            if (includes != null && includes.Any())
                foreach (var include in includes) q = q.Include(include);
            return q;
        }

        // ---------------------------
        // Dynamic
        // ---------------------------

        public async Task<List<TEntity>> GetAllDynamicAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (includes != null)
                foreach (var include in includes) query = query.Include(include);

            if (filter != null) query = query.Where(filter);
            if (orderBy != null) query = orderBy(query);

            return await query.ToListAsync();
        }

        public Task<int> UpdateWhereAsync(Expression<Func<TEntity, bool>> predicate, Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>> setProps)
        {
            throw new NotImplementedException();
        }
    }
}
