using Repository.Layer.Specification;
using System.Linq.Expressions;

namespace Base.Repository
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : class
    {
        IAsyncEnumerable<TEntity> GetAll();
        Task<List<TEntity>> GetAll(Expression<Func<TEntity, bool>> spec);
        Task<List<TEntity>> GetAllAsNoTracking(); // ✅ Added
        Task<List<TEntity>> GetAllAsNoTracking(Expression<Func<TEntity, bool>> spec); // ✅ Added
        Task<IEnumerable<TEntity>> GetAllWithSpecs(ISpecification<TEntity> spec);
        Task<TEntity> GetByIdWithSpecs(ISpecification<TEntity> spec);
        Task<TEntity> GetById(TKey id);
        Task<TEntity> Get(Expression<Func<TEntity, bool>> spec);
        Task Create(TEntity entity);
        Task Update(TEntity entity);
        Task Delete(TEntity entity);
    }
}
