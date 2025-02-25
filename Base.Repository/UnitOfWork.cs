using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repository.Layer.Interfaces;
using System.Collections;

namespace Repository.Layer
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        private TContext _context;
        private Hashtable _repositories;

        public UnitOfWork(TContext context)
        {
            _context = context;
            _repositories = new Hashtable();
        }
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public IGenericRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class
        {
            var entityType = typeof(TEntity);

            if (!_repositories.ContainsKey(entityType))
            {
                var repoType = typeof(GenericRepository<,,>).MakeGenericType(entityType, typeof(TKey), typeof(TContext));
                var repoInstance = Activator.CreateInstance(repoType, _context);
                _repositories[entityType] = repoInstance;
            }

            return (IGenericRepository<TEntity, TKey>)_repositories[entityType];
        }
    }
}
