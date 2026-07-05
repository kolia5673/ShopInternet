using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopInternet.DataAccess.Repository.IRepository;
using ShopInternet.Helpers;

namespace ShopInternet.Controllers;

[Route("api/product")]
[ApiController]
public class ProductApiController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ProductApiController> _logger;

    public ProductApiController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, ILogger<ProductApiController> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    private async Task<bool> IsValidKey(string key)
    {
        var users = await _userManager.GetUsersForClaimAsync(new Claim("ApiKey", key));
        var user = users.FirstOrDefault();
        if (user != null)
        {
            _logger.LogInformation($"[DEBUG_LOG] API Request: {user.UserName}, Controller: {nameof(ProductApiController)}");
            Console.WriteLine($"{user.UserName}, Controller: {nameof(ProductApiController)}");
            return true;
        }

        return false;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(string apiKey, string? productName = null, string? categoryName = null)
    {
        if (!await IsValidKey(apiKey)) return Unauthorized("Invalid API key");
        
        var products = await _unitOfWork.Product.GetAllAsync(includePropertices: "Category");

        if (!string.IsNullOrEmpty(productName))
        {
            products = products.Where(p => p.Name.Contains(productName));
        }
        if (!string.IsNullOrEmpty(categoryName))
        {
            products = products.Where(p => p.Category.Name.Contains(categoryName));
        }

        var productList = products.ToList();
        AppHelper.ConverImagePathToURL(productList, Request);
        return Ok(productList);
    }

    [HttpGet("category/{id:int}")]
    public async Task<IActionResult> GetCategoryById(int id, string apiKey)
    {
        if(!await IsValidKey(apiKey)) return Unauthorized("Invalid API key");
        var category = await _unitOfWork.Category.GetFirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return NotFound();
        var products = await _unitOfWork.Product.GetAllAsync(x => x.CategoryId == id);
        var productList = products.ToList();
        AppHelper.ConverImagePathToURL(productList, Request);
        return Ok(productList);
    }

}