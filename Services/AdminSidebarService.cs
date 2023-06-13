using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace _06_MvcWeb.Services
{
	public class AdminSidebarService
	{
        public string test { get; set; }
        public AdminSidebarService()
		{

        }
        public string SingleTag(string content, string url,string icon, params string[] properties)
        {
            icon += " ";
            string propertiesStr = " ";
            foreach (var prop in properties)
            {
                propertiesStr += prop;
            }
            string html = @"<li class=""nav-item"">
								<a class=""nav-link"" href=""" + url + @""" " + propertiesStr + @">
								"+ icon + @"<span>" + content + @"</span>
								</a>
							</li>";
            return html;
        }
        string Divider() => @"<hr class=""sidebar-divider my-0"">";
        public string GroupTag(string content, string collapseId,string icon,params string[] childTags)
        {
            icon += " ";
            string html = @"<li class=""nav-item"">
                      <a class=""nav-link collapsed"" href=""#"" data-bs-toggle=""collapse"" data-bs-target=""#" + collapseId + @"""
                         aria-expanded=""true"" aria-controls=""" + collapseId + @""">
                          "+icon+@"
                          <span>" + content + @"</span>
                      </a>
                      <div id=""" + collapseId + @""" class=""collapse"" aria-labelledby=""" + collapseId + @""" data-parent=""#accordionSidebar"">
                          <div class=""bg-white py-2 collapse-inner rounded"">";
            string html2 = "";
            foreach (string childTag in childTags)
            {
                html2 += childTag;
            }
            string html3 = @"</div>
                      </div>
                  </li>";
            return html + html2 + html3;
        }
        public string ChildTag(string content, string url)
        {
            return @"<a class=""collapse-item"" href=""" + url + @""">" + content + @"</a>";
        }
        public string ChildHeaderTag(string content)
        {
            return @"<h6 class=""collapse-header"">"+content+@"</h6>";
        }
        public string Gerenate(IUrlHelper url)
        {
            string html = "";

            html += Divider();
            html += SingleTag("Quản lý database", url.Action("Index", "DbManage", new { Area = "Database" }), @"<i class=""fas fa-database""></i>");
            html += SingleTag("Quản lý liên hệ", url.Action("Index", "Contact", new { Area = "Contact" }), @"<i class=""far fa-id-badge""></i>");
            html += SingleTag("Quản lý File", url.Action("Index", "FileManager", new { Area = "Files" }),@"<i class=""fas fa-folder-open""></i>", @"target=""_blank""");

            html += Divider();
            html += GroupTag(content:"Users And Roles",collapseId:"userandrole",icon: @"<i class=""fas fa-user-tag""></i>",
                ChildTag("Roles", url.Action("Index", "Role", new { Area = "Identity" })),
                ChildTag("Users", url.Action("Index", "User", new { Area = "Identity" }))
                );

            html += Divider();
            html += GroupTag("Bài viết và sản phẩm", "blogandproduct", icon: @"<i class=""fas fa-cart-plus""></i>",
                ChildHeaderTag("Bài viết"),
                ChildTag("Các bài viết", url.Action("Index", "Post", new { Area = "Blog" })),
                ChildTag("Các chuyên mục", url.Action("Index", "Category", new { Area = "Blog" })),
                ChildHeaderTag("Sản phẩm"),
                ChildTag("Các sản phẩm", url.Action("Index", "Product", new { Area = "Products" })),
                ChildTag("Các chuyên mục", url.Action("Index", "Category", new { Area = "Products" }))
                );

            return html;
        }
    }
}
