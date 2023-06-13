using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using _06_MvcWeb.Blog.Models;
using _06_MvcWeb.Models;
using Microsoft.AspNetCore.Authorization;
using _06_MvcWeb.Data;
using Microsoft.AspNetCore.Identity;
using _06_MvcWeb.Utilities;

namespace _06_MvcWeb.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/post/[action]/{id?}")]
    [Authorize(Roles =RoleName.Administrator+","+RoleName.Editor)]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PostController(AppDbContext context, UserManager<AppUser> userManager)
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
            
            int totalPosts = await _context.Posts.CountAsync();               //Lấy items count
            
            Page.currentPage = p.Value;
            Page.countPages = (int)Math.Ceiling((double)totalPosts / pagesize.Value);
            if (Page.currentPage < 1) Page.currentPage = 1;
            if (Page.currentPage > Page.countPages) Page.currentPage = Page.countPages;
            Page.generateUrl = (int? page) => @Url.Action("Index", "Post", new { Area = "Blog", p = page , pagesize = pagesize.Value });
            ViewBag.Page = Page;
            ViewBag.pagesize = pagesize.Value;
            if (totalPosts == 0) return View();
            var posts = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.PostsAndCategories).ThenInclude(pc=>pc.PostCategory)
                .OrderByDescending(p => p.DateUpdated)
                .Skip((Page.currentPage - 1) * pagesize.Value)
                .Take(pagesize.Value);
            ViewBag.itemIndex = (Page.currentPage - 1) * pagesize.Value;
            
            return View(await posts.ToListAsync());
        }

        //  ---------------------------------------------------------------  DETAIL ---------------------------------------------------------------
        //  ---------------------------------------------------------------  DETAIL ---------------------------------------------------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        //  ---------------------------------------------------------------  CREATE ---------------------------------------------------------------
        //  ---------------------------------------------------------------  CREATE ---------------------------------------------------------------
        public async Task<IActionResult> Create()
        {
            var categories = await _context.PostCategories.ToListAsync();
            ViewBag.CategorySelectList = new MultiSelectList(categories, "Id", "Title");
            return View();
        }
        //  --------------------------------------------------  POST CREATE -------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryIds")] CreatePostModel post)
        {
            ModelState.Remove("Author");
            ModelState.Remove("AuthorId");
            ModelState.Remove("PostsAndCategories");
            ModelState.Remove("CategoryIds");
            
            var categories = await _context.PostCategories.ToListAsync();
            ViewBag.CategorySelectList = new MultiSelectList(categories, "Id", "Title");

            post.Slug ??= AppUtilities.GenerateSlug(post.Title);
            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError(string.Empty, "Nhập chuỗi url khác");
            }
            if (ModelState.IsValid)
            {
                
                post.DateCreated = post.DateUpdated = DateTime.Now;
                var user = await _userManager.GetUserAsync(User);
                post.AuthorId = user.Id;

                _context.Posts.Add(post);
                if (post.CategoryIds != null)
                    foreach (var CategoryId in post.CategoryIds)
                    {
                        _context.Add(new PostsAndCategories
                        {
                            CategoryId = CategoryId,
                            Post = post
                        });
                    }
                await _context.SaveChangesAsync();
                StatusMessage = $"Vừa tạo bài viết <strong>{post.Title}</strong>";
                return RedirectToAction(nameof(Index));
            }
            
            return View(post);
        }

        //  ---------------------------------------------------------------  EDIT ---------------------------------------------------------------
        //  ---------------------------------------------------------------  GET ---------------------------------------------------------------
        public async Task<IActionResult> Edit(int? id, int? p, int? pagesize)
        {
            var categories = await _context.PostCategories.ToListAsync();
            ViewBag.CategorySelectList = new MultiSelectList(categories, "Id", "Title");
            ViewBag.p = p; ViewBag.pagesize = pagesize;
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p=>p.PostsAndCategories)
                .FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            var postEdit = new CreatePostModel() {
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                Published = post.Published,
                CategoryIds = post.PostsAndCategories.Select(pc => pc.CategoryId).ToArray()
            };



            return View(postEdit);
        }
        //  ---------------------------------------------------------------  POST:EDIT ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published,CategoryIds")] CreatePostModel post,int?p,int? pagesize)
        {
            ModelState.Remove("Author");
            ModelState.Remove("AuthorId");
            ModelState.Remove("PostsAndCategories");
            ModelState.Remove("CategoryIds");
            var categories = await _context.PostCategories.ToListAsync();
            ViewBag.CategorySelectList = new MultiSelectList(categories, "Id", "Title");
            ViewBag.p = p;ViewBag.pagesize = pagesize;
            post.Slug ??= AppUtilities.GenerateSlug(post.Title);
            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && post.PostId != p.PostId))
            {
                ModelState.AddModelError(string.Empty, "Nhập chuỗi url khác");
            }
            if (id != post.PostId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Posts
                        .Include(p => p.PostsAndCategories)
                        .FirstOrDefaultAsync(p => p.PostId == id);
                    if (postUpdate==null)
                    {
                        return NotFound();
                    }
                    postUpdate.Title = post.Title;
                    postUpdate.Description = post.Description;
                    postUpdate.Content = post.Content;
                    postUpdate.Published = post.Published;
                    postUpdate.Slug = post.Slug;
                    postUpdate.DateUpdated = DateTime.Now;

                    var oldCateIds = postUpdate.PostsAndCategories.Select(pc => pc.CategoryId).ToArray();
                    var newCateIds = post.CategoryIds;

                    var tobBeAddedIds = newCateIds.Where(newID => !oldCateIds.Contains(newID)).ToArray();
                    var tobBeDeleteIds = oldCateIds.Where(oldID => !newCateIds.Contains(oldID)).ToArray();


                    _context.AddRange(tobBeAddedIds.Select(cateId => new PostsAndCategories() { PostId = postUpdate.PostId, CategoryId = cateId }));
                    _context.RemoveRange(_context.PostsAndCategories.Where(pc => pc.PostId == postUpdate.PostId && tobBeDeleteIds.Contains(pc.CategoryId)));
                    _context.Update(postUpdate);
                    await _context.SaveChangesAsync();
                    StatusMessage = $"Cập nhật thành công {postUpdate.PostId}";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
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
            return View(post);
        }

        //  ---------------------------------------------------------------  DELETE ---------------------------------------------------------------
        //  ---------------------------------------------------------------  DELETE ---------------------------------------------------------------
        public async Task<IActionResult> Delete(int? id,string? returnUrl=null)
        {
            returnUrl ??= Url.Action("Index", "Post");
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }
            ViewBag.returnUrl = returnUrl;
            return View(post);
        }

        //  ---------------------------------------------------------------  POST:DELETE ---------------------------------------------------------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string? returnUrl = null)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'AppDbContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            
            await _context.SaveChangesAsync();
            returnUrl ??= Url.Action("Index", "Post");
            StatusMessage = "Xóa thành công <strong>"+post.Title+"</strong>";
            return Redirect(returnUrl);
        }

        private bool PostExists(int id)
        {
          return (_context.Posts?.Any(e => e.PostId == id)).GetValueOrDefault();
        }
    }
}
