using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _06_MvcWeb.Blog.Models;
using _06_MvcWeb.Models;
using _06_MvcWeb.Data;
using Microsoft.AspNetCore.Authorization;
namespace _06_MvcWeb.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/category/{action}/{id?}")]
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
            var qr = await (from c in _context.PostCategories select c)
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
            var category = await _context.PostCategories
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
        private void CreateSelectItems(List<PostCategory> source, List<PostCategory> des, int level)
        {
            const char SPLIT_CHAR = ' ';
            foreach(var c in source)
            {
                
                des.Add(new PostCategory()
                { 
                    Id = c.Id,
                    Title= string.Concat(Enumerable.Repeat(SPLIT_CHAR, level * 5)) + c.Title
            });
                if (c.CategoryChildren?.Count > 0)
                {
                    CreateSelectItems(c.CategoryChildren.ToList(), des, level + 1);
                }
            }
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var qr = await (from c in _context.PostCategories select c)
                .Include(c => c.ParentCategory)
                .Include(c => c.CategoryChildren).ToListAsync();
            var categories = qr.Where(c => c.ParentCategory == null).ToList();
            var items = new List<PostCategory>();
            CreateSelectItems(categories, items, 0);
            var list = new SelectList(items, "Id", "Title").ToList();
            list.Insert(0, new SelectListItem("Không danh mục", "0"));


            ViewData["ParentCategoryId"] = list;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Slug,ParentCategoryId")] PostCategory category)
        {
            if (category.ParentCategoryId == 0) category.ParentCategoryId = null;
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // State Not Valid: Create Fail
            var qr = await (from c in _context.PostCategories select c)
                .Include(c => c.ParentCategory)
                .Include(c => c.CategoryChildren).ToListAsync();
            var categories = qr.Where(c => c.ParentCategory == null).ToList();
            var items = new List<PostCategory>();
            CreateSelectItems(categories, items, 0);
            var list = new SelectList(items, "Id", "Title").ToList();
            list.Insert(0, new SelectListItem("Không danh mục", "0"));

            ViewData["ParentCategoryId"] = list;
            return View(category);
        }

        //  ---------------------------------------------------------------  EDIT ---------------------------------------------------------------
        //  ---------------------------------------------------------------  EDIT ---------------------------------------------------------------
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PostCategories == null)
            {
                return NotFound();
            }

            var category = await _context.PostCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var qr = await (from c in _context.PostCategories select c)
                .Include(c => c.ParentCategory)
                .Include(c => c.CategoryChildren).ToListAsync();
            var categories = qr.Where(c => c.ParentCategory == null).ToList();
            var items = new List<PostCategory>();
            CreateSelectItems(categories, items, 0);
            var list = new SelectList(items, "Id", "Title").ToList();
            list.Insert(0, new SelectListItem("Không danh mục", "0"));
            ViewData["ParentCategoryId"] = list;
            return View(category);
        }
        
        //  ---------------------------------------------------------------  EDIT POST  ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Slug,ParentCategoryId")] PostCategory category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }
            
            if (category.ParentCategoryId == category.Id) ModelState.AddModelError(string.Empty, "Phải chọn danh mục cha khác");

            category.CategoryChildren = (from cate in _context.PostCategories select cate)
                                    .Where(c => c.Id != category.Id)
                                    .ToList()
                                    .Where(c => c.ParentCategoryId == category.Id)
                                    .ToList();

            Action<PostCategory> checkCondition = null;
            checkCondition = (cate) => {
                if (cate.CategoryChildren != null)
                {
                    foreach(var c in cate.CategoryChildren)
                    {
                        if (category.ParentCategoryId == c.Id)
                        {
                            ModelState.AddModelError(string.Empty, "Danh mục cha không được là các danh mục con cúa chính nó");
                            return;
                        }
                        checkCondition(c);
                    }
                }
            };
            checkCondition(category);
            

            if (ModelState.IsValid&& category.ParentCategoryId != category.Id)
            {
                try
                {
                    if (category.ParentCategoryId == 0) category.ParentCategoryId = null;
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            var qr = await (from c in _context.PostCategories select c)
                .Include(c => c.ParentCategory)
                .Include(c => c.CategoryChildren).ToListAsync();
            var categories = qr.Where(c => c.ParentCategory == null).ToList();
            var items = new List<PostCategory>();
            CreateSelectItems(categories, items, 0);
            var list = new SelectList(items, "Id", "Title").ToList();
            list.Insert(0, new SelectListItem("Không danh mục", "0"));
            ViewData["ParentCategoryId"] = list;
            return View(category);
        }

        //  ---------------------------------------------------------------  DELETE ---------------------------------------------------------------
        //  ---------------------------------------------------------------  DELETE ---------------------------------------------------------------
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PostCategories == null)
            {
                return NotFound();
            }

            var category = await _context.PostCategories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PostCategories == null)
            {
                return Problem("Entity set 'AppDbContext.Categories'  is null.");
            }
            var category = await _context.PostCategories
                            .Include(c => c.CategoryChildren)
                            .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            foreach (var cCategory in category.CategoryChildren)
            {
                cCategory.ParentCategoryId = category.ParentCategoryId;
            }
            _context.PostCategories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //  ---------------------------------------------------------------  Exist ---------------------------------------------------------------
        //  ---------------------------------------------------------------  Exist ---------------------------------------------------------------
        private bool CategoryExists(int id)
        {
          return (_context.PostCategories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
