using ShopInternet.DataAccess.Models;

namespace ShopInternet.DataAccess.Repository.IRepository;

public interface IUnitOfWork
{
    ICategoryRepository Category { get; set; }
    IProductRepository Product { get; set; }
    IRepository<OrderHeader> OrderHeader { get; }
    IRepository<OrderDetails> OrderDetails { get; }
    Task SaveAsync();
}