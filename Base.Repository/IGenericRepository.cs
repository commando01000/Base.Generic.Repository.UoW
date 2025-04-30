using Repository.Layer.Specification;
using System.Linq.Expressions;

namespace Base.Repository
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : class
    {
        IAsyncEnumerable<TEntity> GetAll();
        Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> spec);
        Task<List<TEntity>> GetAllAsNoTracking();
        Task<List<TEntity>> GetAllAsNoTracking(ISpecification<TEntity> spec);
        Task<List<TEntity>> GetAllAsNoTracking(Expression<Func<TEntity, bool>> spec);
        Task<IEnumerable<TEntity>> GetAllWithSpecs(ISpecification<TEntity> spec);
        Task<int> GetCountAsync(ISpecification<TEntity> spec);
        Task<TEntity> GetWithSpecs(ISpecification<TEntity> spec);
        Task<TEntity> Get(TKey id);
        Task<TEntity> Get(Expression<Func<TEntity, bool>> spec);
        Task<Guid> Create(TEntity entity);  // Changed to Task<Guid>
        Task<bool> Update(TEntity entity);  // Remains bool
        Task<bool> Delete(TEntity entity);  // Remains bool
    }
}