using Repository.Layer.Specification;
using System.Linq.Expressions;

namespace Repository.Layer.Interfaces
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : class
    {
        public IAsyncEnumerable<TEntity> GetAll();
        public Task<IEnumerable<TEntity>> GetAllWithSpecs(ISpecification<TEntity> spec);
        public Task<TEntity> GetById(TKey id);
        // add a function that takes lambda expression
        public Task<TEntity> Get(Expression<Func<TEntity, bool>> spec);
        public Task<TEntity> GetByIdWithSpecs(ISpecification<TEntity> spec);
        public Task Create(TEntity entity);
        public Task Update(TEntity entity);
        public Task Delete(TEntity entity);
    }
}
