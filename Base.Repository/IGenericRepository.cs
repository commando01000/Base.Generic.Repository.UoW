using Base.Repository.Utilities;
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
        Task<PaginatedResult<TEntity>> GetAllPaginatedAsNoTrackingAsync(int pageIndex, int pageSize);
        Task<PaginatedResult<TEntity>> GetAllWithSpecsPaginatedAsync(ISpecification<TEntity> spec, int pageIndex, int pageSize);

        Task<List<TEntity>> GetAllWithIncludesAsync(params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity> GetByIdWithIncludesAsync(TKey id, params Expression<Func<TEntity, object>>[] includes);
        Task<PaginatedResult<TEntity>> GetAllWithIncludesPaginatedAsync(int pageIndex, int pageSize, params Expression<Func<TEntity, object>>[] includes);
        Task<PaginatedResult<TEntity>> GetAllWithIncludesPaginatedAsNoTrackingAsync(int pageIndex, int pageSize, params Expression<Func<TEntity, object>>[] includes);
        Task<PaginatedResult<TEntity>> GetAllPaginatedAsync(int pageIndex, int pageSize);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> GetCountAsync(ISpecification<TEntity> spec);
        Task<TEntity> GetWithSpecs(ISpecification<TEntity> spec);
        Task<TEntity> Get(TKey id);
        Task<TEntity> Get(Expression<Func<TEntity, bool>> spec);

        Task<Guid> Create(TEntity entity);
        Task BulkInsertAsync(IEnumerable<TEntity> entities);
        Task<bool> Update(TEntity entity);
        Task<bool> Delete(TEntity entity);
        Task<bool> SoftDeleteAsync(TKey id);
        Task<List<TEntity>> GetAllDynamicAsync(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params Expression<Func<TEntity, object>>[] includes);
    }
}
