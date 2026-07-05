using ShopInternet.DataAccess.Models;

namespace ShopInternet.DataAccess.Repository.IRepository;

public interface ICategoryRepository : IRepository<Category>
{
    void Update(Category obj);
}