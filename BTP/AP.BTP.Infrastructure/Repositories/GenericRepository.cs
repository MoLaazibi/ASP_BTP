using AP.BTP.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AP.BTP.Infrastructure.Repositories
{
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly DbSet<T> _dbSet;

        protected GenericRepository(DbContext context)
        {
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAll(int pageNr, int pageSize)
        {
            return await _dbSet.Skip((pageNr - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<T> GetById(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> Create(T newItem)
        {
            await _dbSet.AddAsync(newItem);
            return newItem;
        }

        public async Task Delete(T item)
        {
            _dbSet.Remove(item);
            await Task.CompletedTask;
        }

        public async Task<T> Update(T modifiedItem)
        {
            _dbSet.Update(modifiedItem);
            return modifiedItem;
        }
        public async Task<int> CountAsync() => await _dbSet.CountAsync();
        public async Task<T> FindAsync(Expression<Func<T, bool>> predicate) => await _dbSet.FirstOrDefaultAsync(predicate);
    }
}
