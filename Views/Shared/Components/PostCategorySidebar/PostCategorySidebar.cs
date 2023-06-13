using _06_MvcWeb.Blog.Models;
using Microsoft.AspNetCore.Mvc;

namespace _06_MvcWeb.Components
{
    [ViewComponent]
    public class PostCategorySidebar:ViewComponent
    {
        public class CategorySidebarData
        {
            public List<PostCategory> Categories { get; set; }
            public int level { get; set; }
            public string categorySlug { get; set; }
        }
        public IViewComponentResult Invoke(CategorySidebarData data)
        {
            return View(data);
        }
	}
}
