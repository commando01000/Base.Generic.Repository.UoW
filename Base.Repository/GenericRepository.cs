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

        public async Task Create(TEntity entity)
        {
            try
            {
                await _context.Set<TEntity>().AddAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity: {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }

        public async Task Delete(TEntity entity)
        {
            try
            {
                _context.Set<TEntity>().Remove(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity: {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }

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

        // ✅ New function: Returns untracked entities for read-only operations
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
                return new List<TEntity>(); // ✅ Prevents crashes
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

        public async Task<TEntity> GetById(TKey id)
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

        public async Task<TEntity> GetByIdWithSpecs(ISpecification<TEntity> specs)
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

        public async Task Update(TEntity entity)
        {
            try
            {
                _context.Set<TEntity>().Update(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity: {EntityType}", typeof(TEntity).Name);
                throw;
            }
        }
    }
}
