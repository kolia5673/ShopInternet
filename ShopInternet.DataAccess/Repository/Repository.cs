using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShopInternet.DataAccess.Data;
using ShopInternet.DataAccess.Repository.IRepository;

namespace ShopInternet.DataAccess.Repository;

public class Repository<T>: IRepository<T> where T : class
{
    private readonly ShopDbContext _db;
    internal DbSet<T> dbSet;
    public Repository(ShopDbContext db)
    {
        _db = db;
        dbSet = _db.Set<T>();
    }

    public async Task AddAsync(T entity)
    {   
        await dbSet.AddAsync(entity);
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includePropertices = null)
    {
        IQueryable<T> query = dbSet;
        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (includePropertices != null)
        {
            foreach (var includeProp in includePropertices.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp);
            }
        }

        return await query.ToListAsync();
    }

    public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>>? filter = null, string? includePropertices = null, bool tracked = true)
    {
        IQueryable<T> query = tracked ? dbSet : dbSet.AsNoTracking();
        if(filter != null)
        {
            query = query.Where(filter);
        }
        if (includePropertices != null)
        {
            foreach (var includeProp in includePropertices.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp);
            }
        }

        return await query.FirstOrDefaultAsync();
    }

    public void Remove(T entity)
    {
        dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<T> entity)
    {
        dbSet.RemoveRange(entity);
    }
}