using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInternet.Data;
using ShopInternet.Models;
using ShopInternet.Utility;

namespace ShopInternet.Controllers
{
    public class CartController : Controller
    {
        private readonly ShopDbContext _db;

        public CartController(ShopDbContext db)
        {
            _db = db;
        }
        
        // GET: CartController
        public async Task<IActionResult> Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            var sessionCart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            if (sessionCart != null && sessionCart.Count > 0)
            {
                shoppingCartList = sessionCart;
            }

            List<int> productInCart = shoppingCartList.Select(x => x.ProductId).ToList();
            IEnumerable<Product> productList = await _db.Product.Include(x => x.Category).Where(x => productInCart.Contains(x.Id)).ToListAsync();
            
            return View(productList);
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            var sessionCart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            if (sessionCart != null && sessionCart.Count > 0)
            {
                shoppingCartList = sessionCart;
            }

            var itemToRemove = shoppingCartList.FirstOrDefault(x => x.ProductId == id);
            if (itemToRemove != null)
            {
                shoppingCartList.Remove(itemToRemove);
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
    }
}
