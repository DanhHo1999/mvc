using _06_MvcWeb.Blog.Models;
using _06_MvcWeb.Data;
using _06_MvcWeb.Models;
using _06_MvcWeb.Products.Models;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace _06_MvcWeb.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        public DbManageController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult DeleteDB()
        {
            return View();
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpPost]
        [Authorize(Roles = RoleName.Administrator)]
		public async Task<IActionResult> DeleteDBAsync()
		{
            var success=await _context.Database.EnsureDeletedAsync();
            StatusMessage = success ? "Xóa database thành công" : "Không xóa được";
            return RedirectToAction(nameof(Index));
		}
        [HttpPost]
        public async Task<IActionResult> Migrate()
        {
            await _context.Database.MigrateAsync();
            StatusMessage = "Cập nhật database thành công";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> SeedDataAsync()
        {
            var rolesname = typeof(RoleName).GetFields().ToList();
            foreach (var role in rolesname)
            {
                var roleName= (string)role.GetRawConstantValue();
                var rfound = await _roleManager.FindByNameAsync(roleName);
                if(rfound == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            //admin,password123,admin@example.com
            var useradmin = await _userManager.FindByNameAsync("admin");
            if(useradmin == null)
            {
                useradmin = new AppUser()
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(useradmin, "123123");
                await _userManager.AddToRoleAsync(useradmin, RoleName.Administrator);
				await _signInManager.SignInAsync(useradmin, false);
				return RedirectToAction(nameof(SeedDataAsync)); 
			}
            else
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Forbid();
                var roles = await _userManager.GetRolesAsync(user);
                if (!roles.Any(r => r == RoleName.Administrator))
                {
                    return Forbid();
                }
            }



            SeePostCategory();
			_context.SaveChanges();
			SeedProductCategory();
			_context.SaveChanges();
			StatusMessage = "Vừa seed Admin User";
			return RedirectToAction("Index");
		}
        private void SeedProductCategory() 
        {
			_context.Products.RemoveRange(_context.Products.Where(p => p.Content.Contains("fake")));
			_context.ProductCategories.RemoveRange(_context.ProductCategories.Where(c => c.Description.Contains("fake")));
			ProductCategory catea1 = new ProductCategory { Title = "CategoryProduct A1", Description = "A1 fake", Slug = "categoryproduct-a1", ParentCategoryId = null };
			ProductCategory catea1b1 = new ProductCategory { Title = "CategoryProduct A1-B1", Description = "A1B1 fake", Slug = "categoryproduct-a1-b1", ParentCategory = catea1 };
			ProductCategory catea1b2 = new ProductCategory { Title = "CategoryProduct A1-B2", Description = "A1B2 fake", Slug = "categoryproduct-a1-b2", ParentCategory = catea1 };
			ProductCategory catea2 = new ProductCategory { Title = "CategoryProduct A2", Description = "A2 fake", Slug = "categoryproduct-a2", ParentCategory = null };
			ProductCategory catea2b1 = new ProductCategory { Title = "CategoryProduct A2-B1", Description = "A2B1 fake", Slug = "categoryproduct-a2-b1", ParentCategory = catea2 };
			ProductCategory catea2b2 = new ProductCategory { Title = "CategoryProduct A2-B2", Description = "A2B2 fake", Slug = "categoryproduct-a2-b2", ParentCategory = catea2 };
			ProductCategory catea2b3 = new ProductCategory { Title = "CategoryProduct A2-B3", Description = "A2B3 fake", Slug = "categoryproduct-a2-b3", ParentCategory = catea2 };
			ProductCategory catea2b2c1 = new ProductCategory { Title = "CategoryProduct A2-B2-C1", Description = "A2B2C1 fake", Slug = "categoryproduct-a2-b2-c1", ParentCategory = catea2b2 };
			List<ProductCategory> categories = new List<ProductCategory>() { catea1, catea1b1, catea1b2, catea2, catea2b1, catea2b2, catea2b3, catea2b2c1 };
			_context.ProductCategories.AddRange(categories);

			//POST
			var rCateIndex = new Random();
			int bv = 1;

            var user = _userManager.FindByNameAsync("admin").Result;
            
			var fakerProduct = new Faker<Product>();
			fakerProduct.RuleFor(p => p.AuthorId, f => user.Id);
			fakerProduct.RuleFor(p => p.Content, f => f.Commerce.ProductDescription() + " fake");
			fakerProduct.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2021, 1, 1), new DateTime(2021, 7, 11)));
			fakerProduct.RuleFor(p => p.Description, f => f.Lorem.Sentence(3));
			fakerProduct.RuleFor(p => p.Published, f => true);
			fakerProduct.RuleFor(p => p.Slug, f => f.Lorem.Slug());
			fakerProduct.RuleFor(p => p.Title, f => $"Product {bv++} {f.Lorem.Sentence(3, 4).Trim('.')}");
            fakerProduct.RuleFor(p => p.Price, f => int.Parse(f.Commerce.Price(500, 1000, 0)));

			List<Product> products = new List<Product>();
			List<ProductsAndCategories> productsAndCategories = new List<ProductsAndCategories>();

			for (int i = 0; i < 40; i++)
			{
				var product = fakerProduct.Generate();
				product.DateUpdated = product.DateCreated;
				products.Add(product);
				productsAndCategories.Add(new ProductsAndCategories()
				{
					Product = product,
					ProductCategory = categories[rCateIndex.Next(7)]
				});
			}
			_context.AddRange(products);
			_context.AddRange(productsAndCategories);

			_context.SaveChanges();

		}

        private void SeePostCategory()
        {
            _context.Posts.RemoveRange(_context.Posts.Where(p => p.Content.Contains("fake")));
            _context.PostCategories.RemoveRange(_context.PostCategories.Where(c => c.Description.Contains("fake")));
            PostCategory catea1 = new PostCategory { Title = "Category A1", Description = "A1 fake", Slug = "Category-a1", ParentCategoryId = null };
            PostCategory catea1b1 = new PostCategory { Title = "Category A1-B1", Description = "A1B1 fake", Slug = "Category-a1-b1", ParentCategory = catea1 };
            PostCategory catea1b2 = new PostCategory { Title = "Category A1-B2", Description = "A1B2 fake", Slug = "Category-a1-b2", ParentCategory = catea1 };
            PostCategory catea2 = new PostCategory { Title = "Category A2", Description = "A2 fake", Slug = "Category-a2", ParentCategory = null };
            PostCategory catea2b1 = new PostCategory { Title = "Category A2-B1", Description = "A2B1 fake", Slug = "Category-a2-b1", ParentCategory = catea2 };
            PostCategory catea2b2 = new PostCategory { Title = "Category A2-B2", Description = "A2B2 fake", Slug = "Category-a2-b2", ParentCategory = catea2 };
            PostCategory catea2b3 = new PostCategory { Title = "Category A2-B3", Description = "A2B3 fake", Slug = "Category-a2-b3", ParentCategory = catea2 };
            PostCategory catea2b2c1 = new PostCategory { Title = "Category A2-B2-C1", Description = "A2B2C1 fake", Slug = "Category-a2-b2-c1", ParentCategory = catea2b2 };
            List<PostCategory> categories = new List<PostCategory>() { catea1, catea1b1, catea1b2, catea2, catea2b1, catea2b2, catea2b3, catea2b2c1 };
            _context.PostCategories.AddRange(categories);

            //POST
            var rCateIndex = new Random();
            int bv = 1;

			var user = _userManager.FindByNameAsync("admin").Result;
			
            var fakerPost = new Faker<Post>();
            fakerPost.RuleFor(p => p.AuthorId, f => user.Id);
            fakerPost.RuleFor(p => p.Content, f => f.Lorem.Paragraph(7) + "fake");
            fakerPost.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2021,1,1),new DateTime(2021,7,11)));
            fakerPost.RuleFor(p => p.Description, f => f.Lorem.Sentence(3));
            fakerPost.RuleFor(p => p.Published, f => true);
            fakerPost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerPost.RuleFor(p => p.Title, f => $"Post {bv++} {f.Lorem.Sentence(3,4).Trim('.')}");

            List<Post> posts = new List<Post>();
            List<PostsAndCategories> postCategories = new List<PostsAndCategories>();

            for(int i = 0; i < 40; i++)
            {
                var post = fakerPost.Generate();
                post.DateUpdated = post.DateCreated;
                posts.Add(post);
                postCategories.Add(new PostsAndCategories()
                {
                    Post=post,
                    PostCategory= categories[rCateIndex.Next(7)]
                });
            }
            _context.AddRange(posts);
            _context.AddRange(postCategories);

            _context.SaveChanges();
        }
    }
}
