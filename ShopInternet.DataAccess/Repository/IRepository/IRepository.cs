using System.Linq.Expressions;

namespace ShopInternet.DataAccess.Repository.IRepository;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includePropertices = null);
    Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>>? filter = null, string? includePropertices = null, bool tracked = true);
    Task AddAsync(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entity);
}