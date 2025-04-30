using Base.Repository;
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
    }
}