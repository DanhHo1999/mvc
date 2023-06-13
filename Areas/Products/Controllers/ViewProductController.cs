using _06_MvcWeb.Products.Models;
using _06_MvcWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using _06_MvcWeb.Products.Services;

namespace _06_MvcWeb.Products.Controllers
{
    [Area("Products")]
    public class ViewProductController:Controller
    {
        private readonly ILogger<ViewProductController> _logger;
		
        private readonly AppDbContext _context;
		private readonly CartService _cartService;
        public ViewProductController(ILogger<ViewProductController> logger,AppDbContext appDbContext,CartService cartService)
        {
            _logger = logger;
            _context = appDbContext;
			_cartService = cartService;
        }
		public const int ITEMS_PER_PAGE = 6; public PagingModel Page = new PagingModel();

		[Route("/product/{categorySlug?}")]
        public async Task<IActionResult> Index(string categorySlug, int? p = 1, int? pagesize = ITEMS_PER_PAGE)
        {
			if (_context.Products.Count() == 0) return View();
            int itemsCount = 0;
			


			ViewBag.categories = GetCategories();
            ViewBag.categorySlug = categorySlug;
            var category= await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Slug == categorySlug);
            ViewBag.category = category;
            List<int> childCategoryIds=new List<int>();
			
			Action<ProductCategory> addChildId = null;
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

			var allProducts = _context.Products
                .Include(p => p.Author)
				.Include(p=>p.Photos)
                .Include(p => p.ProductsAndCategories).ThenInclude(pc => pc.ProductCategory)
                .AsQueryable();
			allProducts = allProducts.OrderByDescending(p => p.DateUpdated);


			IEnumerable<Product> filteredProducts = null;

			Page.currentPage = p.Value;
			if (category == null)
            {
                itemsCount = allProducts.Count();
				Page.countPages = (int)Math.Ceiling((double)itemsCount / pagesize.Value);
				if (Page.currentPage < 1) Page.currentPage = 1;
				if (Page.currentPage > Page.countPages) Page.currentPage = Page.countPages;
				allProducts =allProducts.Skip((Page.currentPage - 1) * pagesize.Value)
				.Take(pagesize.Value);
			}
            else
			{
				filteredProducts = allProducts.ToList().Where(p => p.ProductsAndCategories.Any(pc => childCategoryIds.Contains(pc.CategoryId)));
				itemsCount = filteredProducts.Count();
				Page.countPages = (int)Math.Ceiling((double)itemsCount / pagesize.Value);
				if (Page.currentPage < 1) Page.currentPage = 1;
				if (Page.currentPage > Page.countPages) Page.currentPage = Page.countPages;
				filteredProducts = filteredProducts.Skip((Page.currentPage - 1) * pagesize.Value)
				.Take(pagesize.Value);
			}
			ViewBag.filteredProducts = filteredProducts;
			
			
			Page.generateUrl = (int? page) => Url.Action("Index", "ViewProduct", new { Area = "Products", p = page, pagesize = pagesize.Value });
			ViewBag.Page = Page;


			return View(allProducts.ToList());
        }
        private List<ProductCategory> GetCategories()
        {
            var categories = _context.ProductCategories
                            .Include(c => c.CategoryChildren)
                            .ToList()
                            .Where(c => c.ParentCategory == null)
							.ToList();
            return categories;

		}
		[Route("/product/{productSlug}.html")]
		public IActionResult Detail(string productSlug)
		{
			ViewBag.categories = GetCategories();
			var product = _context.Products
				.Where(p => p.Slug == productSlug)
				.Include(p => p.Author)
				.Include(p=>p.Photos)
				.Include(p => p.ProductsAndCategories).ThenInclude(pc => pc.ProductCategory)
				.FirstOrDefault();

			if (product == null) return NotFound("Không thấy bài viết");

			ProductCategory category = product.ProductsAndCategories.FirstOrDefault()?.ProductCategory;
			ViewBag.category = category;

			var otherProducts = _context.Products.Where(p => p.ProductsAndCategories.Any(pc => pc.CategoryId == category.Id))
											.Where(p=>p.ProductId!=product.ProductId)
											.OrderByDescending(p=>p.DateUpdated)
											.Take(5).ToList();
			var categoryIds = product.ProductsAndCategories.Select(pc => pc.CategoryId);
			var otherProducts2 = _context.Products
				.Where(p => 
					p.ProductsAndCategories
					.Select(pc => pc.CategoryId)
					.Any(id => categoryIds.Contains(id)))
				.Where(p=>!otherProducts.Select(p2 => p2.ProductId)
				.Contains(p.ProductId))
				.Distinct()
				.Take(5-otherProducts.Count);

			otherProducts.AddRange(otherProducts2.Where(p=>p.ProductId!=product.ProductId));
			ViewBag.otherProducts = otherProducts;

			
			return View(product);
		}
		[Route("addcart/{productid:int}", Name = "addcart")]
		public IActionResult AddToCart([FromRoute] int productid)
		{
			var product = _context.Products
				.Where(p => p.ProductId == productid)
				.FirstOrDefault();
			if (product == null)
				return NotFound("Không có sản phẩm");
			var cart = _cartService.GetCartItems();
			var cartitem = cart.Find(p => p.product.ProductId == productid);
			if (cartitem != null)
			{
				cartitem.quantity++;
			}
			else
			{
				cart.Add(new CartItem() { quantity = 1, product = product });
			}
			_cartService.SaveCartSession(cart);
            return RedirectToAction(nameof(Cart));
		}
		[Route("/cart",Name ="cart")]
		public IActionResult Cart()
		{
			return View(_cartService.GetCartItems());
		}
        [Route("/updatecart", Name = "updatecart")]
        [HttpPost]
        public IActionResult UpdateCart([FromForm] int productid, [FromForm] int quantity)
        {
			if (quantity <= 0) return Ok();
			var cart = _cartService.GetCartItems();
            cart.Find(p => p.product.ProductId == productid).quantity = quantity;
            _cartService.SaveCartSession(cart);
            return Ok();
        }
        [Route("/removecart/{productid:int}", Name = "removecart")]
        public IActionResult RemoveCart([FromRoute] int productid)
        {
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.product.ProductId == productid);
            if (cartitem != null)
            {
                cart.Remove(cartitem);
            }
            _cartService.SaveCartSession(cart);
            return RedirectToAction(nameof(Cart));
        }
    }
}
