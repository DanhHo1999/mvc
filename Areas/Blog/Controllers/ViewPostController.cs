using _06_MvcWeb.Blog.Models;
using _06_MvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace _06_MvcWeb.Blog.Controllers
{
    [Area("Blog")]
    public class ViewPostController:Controller
    {
        private readonly ILogger<ViewPostController> _logger;
        private readonly AppDbContext _context;
        public ViewPostController(ILogger<ViewPostController> logger,AppDbContext appDbContext)
        {
            _logger = logger;
            _context = appDbContext;
        }
		public const int ITEMS_PER_PAGE = 5; public PagingModel Page = new PagingModel();

		[Route("/post/{categorySlug?}")]
        public async Task<IActionResult> Index(string categorySlug, int? p = 1, int? pagesize = ITEMS_PER_PAGE)
        {
            int totalPosts = 0;
			


			ViewBag.categories = GetCategories();
            ViewBag.categorySlug = categorySlug;
            var category= await _context.PostCategories
                .FirstOrDefaultAsync(c => c.Slug == categorySlug);
            ViewBag.category = category;
            List<int> childCategoryIds=new List<int>();
			
			Action<PostCategory> addChildId = null;
            addChildId = (c =>
            {
                if (c.CategoryChildren?.Count > 0)
                {
                    foreach(var childCategory in c.CategoryChildren)
                    {
                        childCategoryIds.Add(childCategory.Id);
                        addChildId(childCategory);
					}
                }
            });
            if(category!=null)
            {
				addChildId(category);
				childCategoryIds.Add(category.Id);
			}
			ViewBag.childCategoryIds= childCategoryIds.ToArray();

			var posts = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.PostsAndCategories).ThenInclude(pc => pc.PostCategory)
                .AsQueryable();
			posts = posts.OrderByDescending(p => p.DateUpdated);


			IEnumerable<Post> postsCategory = null;

			Page.currentPage = p.Value;
			if (category == null)
            {
                totalPosts = posts.Count();
				Page.countPages = (int)Math.Ceiling((double)totalPosts / pagesize.Value);
				if (Page.currentPage < 1) Page.currentPage = 1;
				if (Page.currentPage > Page.countPages) Page.currentPage = Page.countPages;
				posts =posts.Skip((Page.currentPage - 1) * pagesize.Value)
				.Take(pagesize.Value);
			}
            else
			{
				postsCategory = posts.ToList().Where(p => p.PostsAndCategories.Any(pc => childCategoryIds.Contains(pc.CategoryId)));
				totalPosts = postsCategory.Count();
				Page.countPages = (int)Math.Ceiling((double)totalPosts / pagesize.Value);
				if (Page.currentPage < 1) Page.currentPage = 1;
				if (Page.currentPage > Page.countPages) Page.currentPage = Page.countPages;
				postsCategory = postsCategory.Skip((Page.currentPage - 1) * pagesize.Value)
				.Take(pagesize.Value);
			}
			ViewBag.postsCategory = postsCategory;
			
			
			Page.generateUrl = (int? page) => Url.Action("Index", "ViewPost", new { Area = "Blog", p = page, pagesize = pagesize.Value });
			ViewBag.Page = Page;


			return View(posts.ToList());
        }
        private List<PostCategory> GetCategories()
        {
            var categories = _context.PostCategories
                            .Include(c => c.CategoryChildren)
                            .ToList()
                            .Where(c => c.ParentCategory == null)
							.ToList();
            return categories;

		}
		[Route("/post/{postSlug}.html")]
		public IActionResult Detail(string postSlug)
		{
			ViewBag.categories = GetCategories();
			var post = _context.Posts
				.Where(p => p.Slug == postSlug)
				.Include(p => p.Author)
				.Include(p => p.PostsAndCategories).ThenInclude(pc => pc.PostCategory)
				.FirstOrDefault();

			if (post == null) return NotFound("Không thấy bài viết");

			PostCategory category = post.PostsAndCategories.FirstOrDefault()?.PostCategory;
			ViewBag.category = category;

			var otherPosts = _context.Posts.Where(p => p.PostsAndCategories.Any(pc => pc.CategoryId == category.Id))
											.Where(p=>p.PostId!=post.PostId)
											.OrderByDescending(p=>p.DateUpdated)
											.Take(5).ToList();
			var categoryIds = post.PostsAndCategories.Select(pc => pc.CategoryId);
			var otherPosts2 = _context.Posts
				.Where(p => 
					p.PostsAndCategories
					.Select(pc => pc.CategoryId)
					.Any(id => categoryIds.Contains(id)))
				.Where(p=>!otherPosts.Select(p2 => p2.PostId)
				.Contains(p.PostId))
				.Distinct()
				.Take(5-otherPosts.Count);

			otherPosts.AddRange(otherPosts2.Where(p=>p.PostId!=post.PostId));
			ViewBag.otherPosts = otherPosts;

			
			return View(post);
		}
	}
}
