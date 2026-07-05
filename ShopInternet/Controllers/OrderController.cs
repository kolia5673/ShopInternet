using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInternet.DataAccess.Models;
using ShopInternet.DataAccess.Repository.IRepository;
using ShopInternet.Models.ViewModels;
using ShopInternet.Utility;

namespace ShopInternet.Controllers;

public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    [BindProperty]
    public ProductUserVM ProductUserVM { get; set; }
    
    public OrderController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET
    public async Task<IActionResult> Summary()
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        var sessionCart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        if (sessionCart != null && sessionCart.Count > 0)
        {
            shoppingCartList = sessionCart;
        }
        List<int> productInCart = shoppingCartList.Select(x => x.ProductId).ToList();
        IEnumerable<Product> productList = await _unitOfWork.Product.GetAllAsync(x => productInCart.Contains(x.Id));
        ProductUserVM = new ProductUserVM()
        {
            OrderHeader = new OrderHeader()
        };
        foreach (var cart in shoppingCartList)
        {
            Product prodTemp = productList.FirstOrDefault(x => x.Id == cart.ProductId);
            prodTemp.TempCount = cart.Count;
            ProductUserVM.ProductList.Add(prodTemp);
        }
        
        return View(ProductUserVM);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Summary")]
    public async Task<IActionResult> SummaryPost(ProductUserVM productUserVm)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        var sessionCart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        if (sessionCart != null && sessionCart.Count > 0)
        {
            shoppingCartList = sessionCart;
        }
        productUserVm.OrderHeader.OrderDate = DateTime.Now;
        productUserVm.OrderHeader.OrderStatus = WC.StatusInProgress;
        
        await _unitOfWork.OrderHeader.AddAsync(productUserVm.OrderHeader);
        await _unitOfWork.SaveAsync();

        decimal orderTotal = 0;
        foreach (var cart in shoppingCartList)
        {
            Product product = await _unitOfWork.Product.GetFirstOrDefaultAsync(u => u.Id == cart.ProductId);
            OrderDetails orderDetails = new OrderDetails()
            {
                OrderHeadId = productUserVm.OrderHeader.Id,
                ProductId = cart.ProductId,
                Count = cart.Count,
                Price = product.Price
            };
            orderTotal += cart.Count * product.Price;
            await _unitOfWork.OrderDetails.AddAsync(orderDetails);
        }
        
        productUserVm.OrderHeader.OrderTotal = orderTotal;
        await _unitOfWork.SaveAsync();
        
        HttpContext.Session.Clear();
        
        return RedirectToAction(nameof(Confirmation), new { id = productUserVm.OrderHeader.Id });
    }
    
    public IActionResult Confirmation(int id)
    {
        ViewData["Id"] = id.ToString();
        return View();
    }
}