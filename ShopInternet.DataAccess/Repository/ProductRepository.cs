using ShopInternet.DataAccess.Data;
using ShopInternet.DataAccess.Models;
using ShopInternet.DataAccess.Repository.IRepository;

namespace ShopInternet.DataAccess.Repository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ShopDbContext _db;
    public ProductRepository(ShopDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(Product obj)
    {
        var objFromDb = _db.Product.FirstOrDefault(u => u.Id == obj.Id);
        if (objFromDb != null)
        {
            objFromDb.Name = obj.Name;
            objFromDb.Price = obj.Price;
            objFromDb.Description = obj.Description;
            if (obj.Image != null)
            {
                objFromDb.Image = obj.Image;
            }
            objFromDb.CategoryId = obj.CategoryId;
        }
    }
}