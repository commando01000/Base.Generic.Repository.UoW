using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;
using Repository.Layer.Specification;

namespace Repository.Layer
{
    public class GenericRepository<TEntity, TKey, TContext> : IGenericRepository<TEntity, TKey> where TEntity : class where TContext : DbContext
    {
        protected readonly TContext _context;

        public GenericRepository(TContext context)
        {
            _context = context;
        }
        public async Task Create(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public async Task Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public async IAsyncEnumerable<TEntity> GetAll()
        {
            await foreach (var entity in _context.Set<TEntity>().AsAsyncEnumerable())
            {
                yield return entity;
            }
        }

        public async Task<IEnumerable<TEntity>> GetAllWithSpecs(ISpecification<TEntity> specs)
        {
            return await SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsQueryable(), specs).ToListAsync();
        }

        public async Task<TEntity> GetById(TKey id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public async Task<TEntity> GetByIdWithSpecs(ISpecification<TEntity> specs)
        {
            return await SpecificationEvaluator<TEntity>.GetQuery(_context.Set<TEntity>().AsQueryable(), specs).FirstOrDefaultAsync();
        }

        public async Task Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }
    }
}
