using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInternet.Data;
using ShopInternet.Models;
using ShopInternet.Models.ViewModels;
using ShopInternet.Utility;

namespace ShopInternet.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ShopDbContext _db;

    public HomeController(ILogger<HomeController> logger, ShopDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ProductCategoryVM modelVM = new ProductCategoryVM()
        {
            Products = await _db.Product.Include(c => c.Category).ToListAsync(),
            Categories = await _db.Category.ToListAsync()
        };
        return View(modelVM);
    }

    public async Task<IActionResult> Details(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        var sessionCart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        if (sessionCart != null && sessionCart.Count > 0)
        {
            shoppingCartList = sessionCart;
        }

        DetailsVM detailsVM = new DetailsVM()
        {
            Product = await _db.Product.Include(c => c.Category).FirstOrDefaultAsync(x => x.Id == id) ?? new Product(),
            ExistsInCart = false
        };
        if (shoppingCartList.Any(x => x.ProductId == id))
        {
            detailsVM.ExistsInCart = true;
        }

        return View(detailsVM);
    }

    [HttpPost, ActionName("Details")]
    public IActionResult DetailsPost(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        var sessionCart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        if (sessionCart != null && sessionCart.Count > 0)
        {
            shoppingCartList = sessionCart;
        }
        shoppingCartList.Add(new ShoppingCart {ProductId = id});
        HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult RemoveFromCart(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        var sessionCart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        if (sessionCart != null && sessionCart.Count > 0)
        {
            shoppingCartList = sessionCart;
        }
        
        var itemToRemove = shoppingCartList.SingleOrDefault(x => x.ProductId == id);
        if (itemToRemove != null)
        {
            shoppingCartList.Remove(itemToRemove);
        }
        HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}