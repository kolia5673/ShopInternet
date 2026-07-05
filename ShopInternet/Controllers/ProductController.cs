using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopInternet.DataAccess.Models;
using ShopInternet.DataAccess.Repository.IRepository;
using ShopInternet.Interfaces;
using ShopInternet.Utility;

namespace ShopInternet.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUploader _uploader;

        public ProductController(IUnitOfWork unitOfWork, IUploader uploader)
        {
            _unitOfWork = unitOfWork;
            _uploader = uploader;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            var products = await _unitOfWork.Product.GetAllAsync(includePropertices: "Category");
            return View(products);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(m => m.Id == id, includePropertices: "Category");
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CategoryId"] = new SelectList(await _unitOfWork.Category.GetAllAsync(), "Id", "Name");
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,Image,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                var files = Request.Form.Files;
                if (files.Count > 0)
                {
                    product.Image = await _uploader.UploadFile(files[0], WC.ImagePath);
                }

                await _unitOfWork.Product.AddAsync(product);
                await _unitOfWork.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(await _unitOfWork.Category.GetAllAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(u => u.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(await _unitOfWork.Category.GetAllAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Image,CategoryId")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var productDb = await _unitOfWork.Product.GetFirstOrDefaultAsync(p => p.Id == id, tracked: false);
                    if (productDb == null)
                    {
                        return NotFound();
                    }
                    var files = Request.Form.Files;
                    if (files.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(productDb.Image))
                        {
                            _uploader.DeleteFile(WC.ImagePath, productDb.Image);
                        }
                        product.Image = await _uploader.UploadFile(files[0], WC.ImagePath);
                        
                    }
                    else
                    {
                        product.Image = productDb.Image;
                    }

                    _unitOfWork.Product.Update(product);
                    await _unitOfWork.SaveAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw new HttpRequestException("Bad request something wrong :(");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(await _unitOfWork.Category.GetAllAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(m => m.Id == id, includePropertices: "Category");
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _unitOfWork.Product.GetFirstOrDefaultAsync(u => u.Id == id);
            if (product != null)
            {
                _unitOfWork.Product.Remove(product);
                if (!string.IsNullOrEmpty(product.Image))
                {
                    _uploader.DeleteFile(WC.ImagePath, product.Image);
                }
                await _unitOfWork.SaveAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProductExists(int id)
        {
            return await _unitOfWork.Product.GetFirstOrDefaultAsync(e => e.Id == id) != null;
        }
    }
}
