using System.Linq.Expressions;

namespace Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T> AddAsync(T entity);
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task SaveChangesAsync();
    }
}