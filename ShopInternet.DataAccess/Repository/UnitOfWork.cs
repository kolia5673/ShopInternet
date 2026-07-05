using ShopInternet.DataAccess.Data;
using ShopInternet.DataAccess.Models;
using ShopInternet.DataAccess.Repository.IRepository;

namespace ShopInternet.DataAccess.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly ShopDbContext _db;
    public UnitOfWork(ShopDbContext db)
    {
        _db = db;
        Category = new CategoryRepository(_db);
        Product = new ProductRepository(_db);
        OrderHeader = new Repository<OrderHeader>(_db);
        OrderDetails = new Repository<OrderDetails>(_db);
    }
    public ICategoryRepository Category { get; set; }
    public IProductRepository Product { get; set; }
    public IRepository<OrderHeader> OrderHeader { get; }
    public IRepository<OrderDetails> OrderDetails { get; }
    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }
}