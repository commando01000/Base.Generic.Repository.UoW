using Repository.Layer.Specification;

namespace Repository.Layer.Interfaces
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : class
    {
        public IAsyncEnumerable<TEntity> GetAll();
        public Task<IEnumerable<TEntity>> GetAllWithSpecs(ISpecification<TEntity> spec);
        public Task<TEntity> GetById(TKey id);
        public Task<TEntity> GetByIdWithSpecs(ISpecification<TEntity> spec);
        public Task Create(TEntity entity);
        public Task Update(TEntity entity);
        public Task Delete(TEntity entity);
    }
}
