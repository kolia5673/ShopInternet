using ShopInternet.DataAccess.Models;

namespace ShopInternet.DataAccess.Repository.IRepository;

public interface IProductRepository : IRepository<Product>
{
    void Update(Product obj);
}