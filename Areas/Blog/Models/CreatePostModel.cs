using System.ComponentModel.DataAnnotations;

namespace _06_MvcWeb.Blog.Models
{
    public class CreatePostModel:Post
    {
        [Display(Name ="Chuyên mục")]
        public int[] CategoryIds { get; set; }
    }
}
