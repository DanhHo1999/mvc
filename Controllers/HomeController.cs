using _06_MvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace _06_MvcWeb.Controllers
{
	
	public class HomeController : Controller
	{
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger, AppDbContext context)
		{
			_logger = logger;
			_context = context;
		}

		public IActionResult Index()
		{
            var allProducts = _context.Products
                .Include(p => p.Author)
                .Include(p => p.Photos)
                .Include(p => p.ProductsAndCategories).ThenInclude(pc => pc.ProductCategory)
                .AsQueryable();
            allProducts = allProducts.OrderByDescending(p => p.DateUpdated).Take(4);

            var posts = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.PostsAndCategories).ThenInclude(pc => pc.PostCategory)
                .AsQueryable();
            posts = posts.OrderByDescending(p => p.DateUpdated).Take(3);

			ViewBag.products = allProducts;
			ViewBag.posts = posts;

            return View();
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
}