using _06_MvcWeb.Products.Models;
using Microsoft.AspNetCore.Mvc;

namespace _06_MvcWeb.Components
{
    [ViewComponent]
    public class ProductCategorySidebar:ViewComponent
    {
        public class CategorySidebarData
        {
            public List<ProductCategory> Categories { get; set; }
            public int level { get; set; }
            public string categorySlug { get; set; }
        }
        public IViewComponentResult Invoke(CategorySidebarData data)
        {
            return View(data);
        }
	}
}
