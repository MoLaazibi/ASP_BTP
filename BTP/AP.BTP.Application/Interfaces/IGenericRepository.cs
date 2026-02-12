using System.Linq.Expressions;


namespace AP.BTP.Application.Interfaces
{
    public interface IGenericRepository<T>
    {
        Task<IEnumerable<T>> GetAll(int pageNr, int pageSize);

        Task<T> GetById(int id);
        Task<T> Create(T newItem);

        Task<T> Update(T modifiedItem);
        Task Delete(T item);

        Task<int> CountAsync();
        Task<T> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
