using ShopInternet.DataAccess.Data;
using ShopInternet.DataAccess.Models;
using ShopInternet.DataAccess.Repository.IRepository;

namespace ShopInternet.DataAccess.Repository;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private readonly ShopDbContext _db;
    public CategoryRepository(ShopDbContext db) : base(db)
    {
        _db = db;
    }
    public void Update(Category obj)
    {
        _db.Update(obj);
    }
}