using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Server.Data;
using RenzoAgostini.Server.Exceptions;

namespace RenzoAgostini.Server.Repositories.Interfaces
{
    public abstract class RepositoryBase<T>(RenzoAgostiniDbContext context) : IRepository<T> where T : class
    {
        protected readonly DbSet<T> _dbSet = context.Set<T>();

        protected abstract IQueryable<T> IncludeRelated(IQueryable<T> query);

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await IncludeRelated(_dbSet).ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            var idProperty = typeof(T).GetProperty("Id") ??
                throw new ApiException(System.Net.HttpStatusCode.BadRequest, $"Entity {typeof(T).Name} does not have an Id property");

            var parameter = Expression.Parameter(typeof(T), "e");
            var idPredicate = Expression.Lambda<Func<T, bool>>(
                Expression.Equal(
                    Expression.Property(parameter, idProperty),
                    Expression.Constant(id)
                ),
                parameter
            );

            return await IncludeRelated(_dbSet.Where(idPredicate)).FirstOrDefaultAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
            return entity;
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await SaveChangesAsync();

            return entity;
        }

        public virtual Task DeleteAsync(T entity)
        {
            if (context.Entry(entity).State == EntityState.Detached)
                _dbSet.Attach(entity);

            _dbSet.Remove(entity);
            context.SaveChangesAsync();

            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
