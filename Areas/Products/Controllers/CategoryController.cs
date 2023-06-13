using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _06_MvcWeb.Products.Models;
using _06_MvcWeb.Models;
using _06_MvcWeb.Data;
using Microsoft.AspNetCore.Authorization;
namespace _06_MvcWeb.Products.Controllers
{
    [Area("Products")]
    [Route("admin/products/category/{action}/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }


        //  ---------------------------------------------------------------  INDEX ---------------------------------------------------------------
        //  ---------------------------------------------------------------  INDEX ---------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var qr = await (from c in _context.ProductCategories select c)
                .ToListAsync();
            var categories = qr.Where(c => c.ParentCategory == null).ToList();
            return View(categories);
        }

        //  --------------------------------------------------------------- DETAIL ---------------------------------------------------------------
        //  --------------------------------------------------------------- DETAIL ---------------------------------------------------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PostCategories == null)
            {
                return NotFound();
            }
            var category = await _context.ProductCategories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        //  ---------------------------------------------------------------  CREATE ---------------------------------------------------------------
        //  ---------------------------------------------------------------  CREATE ---------------------------------------------------------------
        private void CreateSelectItems(List<ProductCategory> contextSourceList, List<ProductCategory> destinationList, int level)
        {
            const char SPLIT_CHAR = ' ';
            foreach(var c in contextSourceList)
            {
                
                destinationList.Add(new ProductCategory()
                { 
                    Id = c.Id,
                    Title= string.Concat(Enumerable.Repeat(SPLIT_CHAR, level * 5)) + c.Title
            });
                if (c.CategoryChildren?.Count > 0)
                {
                    CreateSelectItems(c.CategoryChildren.ToList(), destinationList, level + 1);
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var qr = await (from c in _context.ProductCategories select c)
                .Include(c => c.ParentCategory)
                .Include(c => c.CategoryChildren).ToListAsync();
            var categories = qr.Where(c => c.ParentCategory == null).ToList();
            var items = new List<ProductCategory>();
            CreateSelectItems(categories, items, 0);
            var list = new SelectList(items, "Id", "Title").ToList();
            list.Insert(0, new SelectListItem("Không danh mục", "0"));


            ViewData["ParentCategoryId"] = list;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Slug,ParentCategoryId")] ProductCategory category_Product)
        {
            if (category_Product.ParentCategoryId == 0) category_Product.ParentCategoryId = null;
            if (ModelState.IsValid)
            {
                _context.Add(category_Product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // State Not Valid: Create Fail
            var qr = await (from c in _context.ProductCategories select c)
                .Include(c => c.ParentCategory)
                .Include(c => c.CategoryChildren).ToListAsync();
            var categories_Product = qr.Where(c => c.ParentCategory == null).ToList();
            var items = new List<ProductCategory>();
            CreateSelectItems(categories_Product, items, 0);
            var list = new SelectList(items, "Id", "Title").ToList();
            list.Insert(0, new SelectListItem("Không danh mục", "0"));

            ViewData["ParentCategoryId"] = list;
            return View(category_Product);
        }

        //  ---------------------------------------------------------------  EDIT ---------------------------------------------------------------
        //  ---------------------------------------------------------------  EDIT ---------------------------------------------------------------
        //GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ProductCategories == null)
            {
                return NotFound();
            }

            var category_Product = await _context.ProductCategories.FindAsync(id);
            if (category_Product == null)
            {
                return NotFound();
            }
            var qr = await (from c in _context.ProductCategories select c)
                .Include(c => c.ParentCategory)
                .Include(c => c.CategoryChildren).ToListAsync();
            var categories_Product = qr.Where(c => c.ParentCategory == null).ToList();
            var items = new List<ProductCategory>();
            CreateSelectItems(categories_Product, items, 0);
            var list = new SelectList(items, "Id", "Title").ToList();
            list.Insert(0, new SelectListItem("Không danh mục", "0"));
            ViewData["ParentCategoryId"] = list;
            return View(category_Product);
        }
        
        //  ---------------------------------------------------------------  EDIT POST  ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Slug,ParentCategoryId")] ProductCategory category_Product)
        {
            if (id != category_Product.Id)
            {
                return NotFound();
            }
            
            if (category_Product.ParentCategoryId == category_Product.Id) ModelState.AddModelError(string.Empty, "Phải chọn danh mục cha khác");

            category_Product.CategoryChildren = (from cate in _context.ProductCategories select cate)
                                    .Where(c => c.Id != category_Product.Id)
                                    .ToList()
                                    .Where(c => c.ParentCategoryId == category_Product.Id)
                                    .ToList();

            Action<ProductCategory> checkCondition = null;
            checkCondition = (cate) => {
                if (cate.CategoryChildren != null)
                {
                    foreach(var c in cate.CategoryChildren)
                    {
                        if (category_Product.ParentCategoryId == c.Id)
                        {
                            ModelState.AddModelError(string.Empty, "Danh mục cha không được là các danh mục con cúa chính nó");
                            return;
                        }
                        checkCondition(c);
                    }
                }
            };
            checkCondition(category_Product);
            

            if (ModelState.IsValid&& category_Product.ParentCategoryId != category_Product.Id)
            {
                try
                {
                    if (category_Product.ParentCategoryId == 0) category_Product.ParentCategoryId = null;
                    _context.Update(category_Product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Category_ProductExists(category_Product.Id))
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
            var qr = await (from c in _context.ProductCategories select c)
                .Include(c => c.ParentCategory)
                .Include(c => c.CategoryChildren).ToListAsync();
            var categories_Product = qr.Where(c => c.ParentCategory == null).ToList();
            var items = new List<ProductCategory>();
            CreateSelectItems(categories_Product, items, 0);
            var list = new SelectList(items, "Id", "Title").ToList();
            list.Insert(0, new SelectListItem("Không danh mục", "0"));
            ViewData["ParentCategoryId"] = list;
            return View(category_Product);
        }

        //  ---------------------------------------------------------------  DELETE ---------------------------------------------------------------
        //  ---------------------------------------------------------------  DELETE ---------------------------------------------------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ProductCategories == null)
            {
                return NotFound();
            }

            var category_Product = await _context.ProductCategories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category_Product == null)
            {
                return NotFound();
            }

            return View(category_Product);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ProductCategories == null)
            {
                return Problem("Entity set 'AppDbContext.Categories'  is null.");
            }
            var category_Product = await _context.ProductCategories
                            .Include(c => c.CategoryChildren)
                            .FirstOrDefaultAsync(c => c.Id == id);
            if (category_Product == null)
            {
                return NotFound();
            }
            foreach (var category_Product_child in category_Product.CategoryChildren)
            {
                category_Product_child.ParentCategoryId = category_Product.ParentCategoryId;
            }
            _context.ProductCategories.Remove(category_Product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //  ---------------------------------------------------------------  Exist ---------------------------------------------------------------
        //  ---------------------------------------------------------------  Exist ---------------------------------------------------------------
        private bool Category_ProductExists(int id)
        {
          return (_context.ProductCategories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
