using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _06_MvcWeb.Products.Models;
using _06_MvcWeb.Models;
using Microsoft.AspNetCore.Authorization;
using _06_MvcWeb.Data;
using Microsoft.AspNetCore.Identity;
using _06_MvcWeb.Utilities;
using System.ComponentModel.DataAnnotations;

namespace _06_MvcWeb.Products.Controllers
{
    
    
	[Area("Products")]
    [Route("admin/products/[action]/{id?}")]
    [Authorize(Roles =RoleName.Administrator+","+RoleName.Editor)]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ProductController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public const int ITEMS_PER_PAGE = 5;public PagingModel Page = new PagingModel();
        [TempData]
        public string StatusMessage { get; set; }
        //  ---------------------------------------------------------------  INDEX ---------------------------------------------------------------
        //  ---------------------------------------------------------------  INDEX ---------------------------------------------------------------
        public async Task<IActionResult> Index(int? p = 1, int? pagesize = ITEMS_PER_PAGE)
        {
            
            int totalProducts = await _context.Products.CountAsync();               //Lấy items count
            Page.currentPage = p.Value;
            Page.countPages = (int)Math.Ceiling((double)totalProducts / pagesize.Value);
            if (Page.currentPage < 1) Page.currentPage = 1;
            if (Page.currentPage > Page.countPages) Page.currentPage = Page.countPages;
            Page.generateUrl = (int? page) => @Url.Action("Index", "Product", new { Area = "Products", p = page , pagesize = pagesize.Value });
            ViewBag.Page = Page;

            var products = _context.Products
                .Include(p => p.Author)
                .Include(p => p.ProductsAndCategories).ThenInclude(pc=>pc.ProductCategory)
                .OrderByDescending(p => p.DateUpdated)
                .Skip((Page.currentPage - 1) * pagesize.Value)
                .Take(pagesize.Value);
            ViewBag.itemIndex = (Page.currentPage - 1) * pagesize.Value;
            ViewBag.pagesize = pagesize.Value;
            return View(await products.ToListAsync());
        }

        //  ---------------------------------------------------------------  DETAIL ---------------------------------------------------------------
        //  ---------------------------------------------------------------  DETAIL ---------------------------------------------------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        //  ---------------------------------------------------------------  CREATE ---------------------------------------------------------------
        //  ---------------------------------------------------------------  CREATE ---------------------------------------------------------------
        public async Task<IActionResult> Create()
        {
            var categories_Product = await _context.ProductCategories.ToListAsync();
            ViewBag.CategorySelectList = new MultiSelectList(categories_Product, "Id", "Title");
            return View();
        }
        //  --------------------------------------------------  PRODUCT CREATE -------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Price,Description,Slug,Content,Published,CategoryIds")] CreateProductModel product)
        {
            ModelState.Remove("Author");
            ModelState.Remove("AuthorId");
            ModelState.Remove("ProductsAndCategories");
            ModelState.Remove("CategoryIds");
			ModelState.Remove("Photos");
			var categories_Product = await _context.ProductCategories.ToListAsync();
            ViewBag.CategorySelectList = new MultiSelectList(categories_Product, "Id", "Title");

            product.Slug ??= AppUtilities.GenerateSlug(product.Title);
            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug))
            {
                ModelState.AddModelError(string.Empty, "Nhập chuỗi url khác");
            }
            if (ModelState.IsValid)
            {
                
                product.DateCreated = product.DateUpdated = DateTime.Now;
                var user = await _userManager.GetUserAsync(User);
                product.AuthorId = user.Id;

                _context.Products.Add(product);
                if (product.CategoryIds != null)
                    foreach (var Category_Product_Id in product.CategoryIds)
                    {
                        _context.Add(new ProductsAndCategories
                        {
                            CategoryId = Category_Product_Id,
                            Product = product
                        });
                    }
                
                await _context.SaveChangesAsync();
                StatusMessage = $"Đã tạo sản phẩm <strong>{product.Title} (ID: {product.ProductId})</strong>";
                return RedirectToAction(nameof(Index));
            }
            
            return View(product);
        }

        //  ---------------------------------------------------------------  EDIT ---------------------------------------------------------------
        //  ---------------------------------------------------------------  GET ---------------------------------------------------------------
        public async Task<IActionResult> Edit(int? id, int? p, int? pagesize)
        {
            var categories_Product = await _context.ProductCategories.ToListAsync();
            ViewBag.CategorySelectList = new MultiSelectList(categories_Product, "Id", "Title");
            ViewBag.p = p; ViewBag.pagesize = pagesize;
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p=>p.ProductsAndCategories)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            var productEdit = new CreateProductModel() {
                ProductId = product.ProductId,
                Title = product.Title,
                Content = product.Content,
                Description = product.Description,
                Slug = product.Slug,
                Published = product.Published,
                Price = product.Price,
                CategoryIds = product.ProductsAndCategories.Select(pc => pc.CategoryId).ToArray()
            };



            return View(productEdit);
        }
        //  ---------------------------------------------------------------  PRODUCT:EDIT ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Price,Title,Description,Slug,Content,Published,CategoryIds")] CreateProductModel product,int?p,int? pagesize)
        {
            ModelState.Remove("Author");
            ModelState.Remove("AuthorId");
            ModelState.Remove("ProductsAndCategories");
            ModelState.Remove("CategoryIds");
            ModelState.Remove("Photos");
            var categories_Product = await _context.ProductCategories.ToListAsync();
            ViewBag.CategorySelectList = new MultiSelectList(categories_Product, "Id", "Title");
            ViewBag.p = p;ViewBag.pagesize = pagesize;
            product.Slug ??= AppUtilities.GenerateSlug(product.Title);
            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug && product.ProductId != p.ProductId))
            {
                ModelState.AddModelError(string.Empty, "Nhập chuỗi url khác");
            }
            if (id != product.ProductId)
            {
                return NotFound($"id:{id} productId:{product.ProductId}");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var productUpdate = await _context.Products
                        .Include(p => p.ProductsAndCategories)
                        .FirstOrDefaultAsync(p => p.ProductId == id);
                    if (productUpdate==null)
                    {
                        return NotFound();
                    } 
                    productUpdate.Title = product.Title;
                    productUpdate.Description = product.Description;
                    productUpdate.Content = product.Content;
                    productUpdate.Published = product.Published;
                    productUpdate.Slug = product.Slug;
                    productUpdate.Price = product.Price;
                    productUpdate.DateUpdated = DateTime.Now;

                    var oldCateIds = productUpdate.ProductsAndCategories.Select(pc => pc.CategoryId).ToArray();
                    var newCateIds = product.CategoryIds;

                    var tobBeAddedIds = newCateIds.Where(newID => !oldCateIds.Contains(newID)).ToArray();
                    var tobBeDeleteIds = oldCateIds.Where(oldID => !newCateIds.Contains(oldID)).ToArray();

                    _context.AddRange(tobBeAddedIds.Select(cateId => new ProductsAndCategories() { ProductId = productUpdate.ProductId, CategoryId = cateId }));
                    _context.RemoveRange(_context.ProductsAndCategories.Where(pc => pc.ProductId == productUpdate.ProductId && tobBeDeleteIds.Contains(pc.CategoryId)));
                    _context.Update(productUpdate);
                    await _context.SaveChangesAsync();
                    StatusMessage = $"Cập nhật thành công {productUpdate.Title} ( ID : {productUpdate.ProductId} )";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExist(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        //  ---------------------------------------------------------------  DELETE ---------------------------------------------------------------
        //  ---------------------------------------------------------------  DELETE ---------------------------------------------------------------
        public async Task<IActionResult> Delete(int? id,string? returnUrl=null)
        {
            returnUrl ??= Url.Action("Index", "Product");
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.returnUrl = returnUrl;
            return View(product);
        }

        //  ---------------------------------------------------------------  POST:DELETE ---------------------------------------------------------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string? returnUrl = null)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'AppDbContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            returnUrl ??= Url.Action("Index", "Product");
            StatusMessage = "Xóa thành công <strong>"+product.Title+"</strong>";
            return Redirect(returnUrl);
        }

        private bool ProductExist(int id)
        {
          return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
		//  ---------------------------------------------------------------  PHOTO ---------------------------------------------------------------
		//  ---------------------------------------------------------------  PHOTO ---------------------------------------------------------------
        [HttpPost]
        
        public async Task<IActionResult> ListPhoto(int id)
        {
            var product = await _context.Products.Include(p => p.Photos).FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
				return Json(new
				{
					success = 0,
					message = "Product Not Found ListPhoto(int id)"
				});
			}
            var listPhoto = product.Photos.Select(photo => new { 
                photoId=photo.PhotoId,
                path="/contents/Products/"+photo.FileName
            });
            return Json(listPhoto);
		}
        
		[HttpPost]
		public async Task<IActionResult> DeletePhoto(int id)
		{
            var photo = await _context.ProductPhotos.FirstOrDefaultAsync(p => p.PhotoId == id);
			if (photo == null)
			{
				return Json(new
				{
					success = 0,
					message = "Photo Not Found DeletePhoto(int id)"
				});
			}
            _context.Remove(photo);
            System.IO.File.Delete("Uploads/Products/" + photo.FileName);
            await _context.SaveChangesAsync();
            return Ok();
		}
		[HttpPost]
		public async Task<IActionResult> UploadPhotoApi(int id, IFormFile fileUpload)
		{
			var product = await _context.Products.Include(p => p.Photos).FirstOrDefaultAsync(p => p.ProductId == id);
			if (product == null) return NotFound("Không có sản phẩm - UploadPhoto(int id)");
			if (fileUpload != null)
			{
				var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(fileUpload.FileName);
                var destinationPath = Path.Combine("Uploads", "Products", fileName);
                destinationPath.WriteLine(true);
				using (var fileStream = new FileStream(destinationPath, FileMode.Create))
				{
					await fileUpload.CopyToAsync(fileStream);
				}
				_context.Add(new ProductPhoto { FileName = fileName, Product = product });
				await _context.SaveChangesAsync();
			}
			return Ok();
		}
	}
}