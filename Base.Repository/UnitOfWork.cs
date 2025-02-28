using Base.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Repository.Layer.Interfaces;
using System.Collections;

namespace Repository.Layer
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        private readonly TContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly Hashtable _repositories;
        private readonly object _lock = new();

        public UnitOfWork(TContext context, IServiceProvider serviceProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _repositories = new Hashtable();
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public IGenericRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class
        {
            var entityType = typeof(TEntity);

            lock (_lock) // 🔒 Ensures thread safety when adding repositories
            {
                if (!_repositories.ContainsKey(entityType))
                {
                    var repoType = typeof(GenericRepository<,,>).MakeGenericType(entityType, typeof(TKey), typeof(TContext));

                    // Resolve logger from IServiceProvider
                    var loggerType = typeof(ILogger<>).MakeGenericType(repoType);
                    var logger = _serviceProvider.GetRequiredService(loggerType);

                    // Create repository instance with context and logger
                    var repoInstance = Activator.CreateInstance(repoType, _context, logger);

                    if (repoInstance != null)
                    {
                        _repositories.Add(entityType, repoInstance);
                    }
                }
            }

            return (IGenericRepository<TEntity, TKey>)_repositories[entityType];
        }
    }
}
