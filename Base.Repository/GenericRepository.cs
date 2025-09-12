using Base.Repository;
using Base.Repository.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repository.Layer.Specification;
using System.Linq.Expressions;

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

        public async Task<Guid> Create(TEntity entity)
        {
            if (entity == null)
            {
                _logger.LogWarning("Attempted to create a null entity: {EntityType}", typeof(TEntity).Name);
                return Guid.Empty; // Return empty Guid for failure
            }

            try
            {
                await _context.Set<TEntity>().AddAsync(entity);

                // Assume TEntity has a Guid Id property and cast it
                var idProperty = entity.GetType().GetProperty("Id");
                if (idProperty == null || idProperty.PropertyType != typeof(Guid))
                {
                    _logger.LogError("Entity {EntityType} does not have a Guid Id property", typeof(TEntity).Name);
                    return Guid.Empty; // Failure if no Guid Id exists
                }

                var id = (Guid)idProperty.GetValue(entity);
                return id; // Return the entity's Guid
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity: {EntityType}", typeof(TEntity).Name);
                return Guid.Empty; // Return empty Guid for failure
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
                return true; // Success if entity is marked for update
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity: {EntityType}", typeof(TEntity).Name);
                return false; // Failure if an exception occurs
            }
        }

        public async Task BulkInsertAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null || !entities.Any())
                return;

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
                return true; // Success if entity is marked for deletion
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity: {EntityType}", typeof(TEntity).Name);
                return false; // Failure if an exception occurs
            }
        }

        public async Task<List<TEntity>> GetAllDynamicAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (includes != null)
            {
                foreach (var include in includes)
                    query = query.Include(include);
            }

            if (filter != null)
                query = query.Where(filter);

            if (orderBy != null)
                query = orderBy(query);

            return await query.ToListAsync();
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


        // Other methods remain unchanged
        public async Task<TEntity> Get(Expression<Func<TEntity, bool>> spec)
        {
            try
            {
                return await _context.Set<TEntity>().FirstOrDefaultAsync(spec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entity by specification: {EntityType}", typeof(TEntity).Name);
                return null;
            }
        }

        public async IAsyncEnumerable<TEntity> GetAll()
        {
            await foreach (var entity in _context.Set<TEntity>().AsAsyncEnumerable())
            {
                yield return entity;
            }
        }

        public async Task<List<TEntity>> GetAllAsNoTracking()
        {
            try
            {
                return await _context.Set<TEntity>().AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all entities (No Tracking): {EntityType}", typeof(TEntity).Name);
                return new List<TEntity>();
            }
        }

        public async Task<List<TEntity>> GetAllAsNoTracking(Expression<Func<TEntity, bool>> spec)
        {
            try
            {
                return await _context.Set<TEntity>().AsNoTracking().Where(spec).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entities (No Tracking) by condition: {EntityType}", typeof(TEntity).Name);
                return new List<TEntity>();
            }
        }

        public async Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> spec)
        {
            try
            {
                return await _context.Set<TEntity>().Where(spec).ToListAsync();
            }
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
                return await SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsQueryable(), specs).ToListAsync();
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
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<PaginatedResult<TEntity>> GetAllWithIncludesPaginatedAsync(int pageIndex, int pageSize, params Expression<Func<TEntity, object>>[] includes)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (includes != null && includes.Any())
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<TEntity>(totalCount, pageIndex, pageSize, items);
        }

        public async Task<TEntity> GetByIdWithIncludesAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (includes != null && includes.Any())
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(e =>
                EF.Property<TKey>(e, "Id").Equals(id)
            );
        }


        public async Task<PaginatedResult<TEntity>> GetAllWithIncludesPaginatedAsNoTrackingAsync(
            int pageIndex,
            int pageSize,
            params Expression<Func<TEntity, object>>[] includes)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 10;

            IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking();

            if (includes != null && includes.Any())
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<TEntity>(totalCount, pageIndex, pageSize, items);
        }


        public async Task<TEntity> Get(TKey id)
        {
            try
            {
                return await _context.Set<TEntity>().FindAsync(id);
            }
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
                return await SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsQueryable(), specs).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching entity with specifications: {EntityType}", typeof(TEntity).Name);
                return null;
            }
        }

        public async Task<int> GetCountAsync(ISpecification<TEntity> specs)
        {
            return await SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>(), specs).CountAsync();
        }

        public async Task<List<TEntity>> GetAllAsNoTracking(ISpecification<TEntity> spec)
        {
            return await SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsNoTracking(), spec).ToListAsync();
        }

        public async Task<PaginatedResult<TEntity>> GetAllPaginatedAsNoTrackingAsync(int pageIndex, int pageSize)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _context.Set<TEntity>().AsNoTracking();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<TEntity>(totalCount, pageIndex, pageSize, items);
        }

        public async Task<PaginatedResult<TEntity>> GetAllWithSpecsPaginatedAsync(
            ISpecification<TEntity> spec,
            int pageIndex,
            int pageSize)
        {
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsQueryable(), spec);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<TEntity>(totalCount, pageIndex, pageSize, items);
        }


        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _context.Set<TEntity>().AnyAsync(predicate);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Set<TEntity>().CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _context.Set<TEntity>().CountAsync(predicate);
        }

    }
}