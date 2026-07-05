using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInternet.DataAccess.Models;
using ShopInternet.DataAccess.Repository.IRepository;
using ShopInternet.Models.ViewModels;
using ShopInternet.Utility;

namespace ShopInternet.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        ProductCategoryVM modelVM = new ProductCategoryVM()
        {
            Products = await _unitOfWork.Product.GetAllAsync(includePropertices: "Category"),
            Categories = await _unitOfWork.Category.GetAllAsync()
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
            Product = await _unitOfWork.Product.GetFirstOrDefaultAsync(x => x.Id == id, includePropertices: "Category") ?? new Product(),
            ExistsInCart = false
        };

        var itemInCart = shoppingCartList.FirstOrDefault(x => x.ProductId == id);
        if (itemInCart != null)
        {
            detailsVM.ExistsInCart = true;
            detailsVM.Product.TempCount = itemInCart.Count;
        }
        else
        {
            detailsVM.Product.TempCount = 1;
        }

        return View(detailsVM);
    }

    [HttpPost, ActionName("Details")]
    public IActionResult DetailsPost(int id, DetailsVM detailsVM)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        var sessionCart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        if (sessionCart != null && sessionCart.Count > 0)
        {
            shoppingCartList = sessionCart;
        }

        int count = detailsVM.Product.TempCount;
        if (count < 1)
        {
            count = 1;
        }

        shoppingCartList.Add(new ShoppingCart { ProductId = id, Count = count });
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