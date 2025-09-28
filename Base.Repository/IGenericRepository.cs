using Base.Repository.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Repository.Layer.Specification;
using System.Data;
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
        Task<PaginatedResult<TEntity>> GetAllPaginatedAsNoTrackingAsync(
            int pageIndex,
            int pageSize,
            params Expression<Func<TEntity, bool>>[] predicates);

        Task<PaginatedResult<TEntity>> GetAllWithSpecsPaginatedAsync(ISpecification<TEntity> spec, int pageIndex, int pageSize);
        Task<List<TEntity>> GetAllWithIncludesAsync(params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity> GetByIdWithIncludesAsync(TKey id, params Expression<Func<TEntity, object>>[] includes);
        Task<PaginatedResult<TEntity>> GetAllWithIncludesPaginatedAsync(int pageIndex, int pageSize, params Expression<Func<TEntity, object>>[] includes);
        Task<PaginatedResult<TEntity>> GetAllWithIncludesPaginatedAsNoTrackingAsync(int pageIndex, int pageSize, params Expression<Func<TEntity, object>>[] includes);
        Task<List<TEntity>> GetAllDynamicAsync(Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params Expression<Func<TEntity, object>>[] includes);
        Task<PaginatedResult<TEntity>> GetAllPaginatedAsync(int pageIndex, int pageSize);

        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountSpecsAsync(ISpecification<TEntity> spec);
        Task<TEntity> GetWithSpecs(ISpecification<TEntity> spec);
        Task<TEntity> Get(TKey id);
        Task<TEntity> Get(Expression<Func<TEntity, bool>> spec);

        Task<Guid> Create(TEntity entity);
        Task BulkInsertAsync(IEnumerable<TEntity> entities);
        Task<bool> Update(TEntity entity);
        Task<bool> Delete(TEntity entity);
        Task<bool> SoftDeleteAsync(TKey id);

        // ===== Added helpers (non-breaking) =====

        // Single-item helpers with includes and AsNoTracking toggle
        Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity?> SingleOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes);

        // List + optional ordering/includes + AsNoTracking
        Task<List<TEntity>> ListAsync(
            Expression<Func<TEntity, bool>> predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes);

        // Projection (DTO) list
        Task<List<TOut>> ListSelectAsync<TOut>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TOut>> selector,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes);

        // Pagination + projection (DTO) in one go
        Task<PaginatedResult<TOut>> GetPageSelectAsync<TOut>(
            int pageIndex,
            int pageSize,
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TOut>> selector,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool asNoTracking = true,
            params Expression<Func<TEntity, object>>[] includes) where TOut : class;   // <-- add this;

        // Set-based operations (EF Core 7+/8)
        Task<int> DeleteWhereAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> UpdateWhereAsync(
            Expression<Func<TEntity, bool>> predicate,
            Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>> setProps);

        // Soft-delete bulk helper
        Task<int> SoftDeleteWhereAsync(Expression<Func<TEntity, bool>> predicate, string isDeletedProperty = "IsDeleted");

        // Identity helpers
        Task<bool> ExistsByIdAsync(TKey id);
        Task<List<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids, bool asNoTracking = true);

        // Seek (cursor) pagination for large datasets
        Task<List<TEntity>> GetSliceAsync<TOrderKey>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TOrderKey>> orderBy,
            TOrderKey afterKey,
            int take,
            bool ascending = true,
            bool asNoTracking = true);

        // Transactions & UoW
        Task ExecuteInTransactionAsync(Func<Task> action, IsolationLevel? isolation = null);
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        // Escape hatch for advanced queries
        IQueryable<TEntity> Query(bool asNoTracking = true, params Expression<Func<TEntity, object>>[] includes);
    }
}
