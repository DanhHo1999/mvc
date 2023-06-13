using System.ComponentModel.DataAnnotations.Schema;

namespace _06_MvcWeb.Blog.Models
{
    [Table("PostsAndCategories")]
    public class PostsAndCategories
    {
        public int PostId { set; get; }

        public int CategoryId { set; get; }

        [ForeignKey("PostId")]
        public Post Post { set; get; }

        [ForeignKey("CategoryId")]
        public PostCategory PostCategory{ set; get; }
    }

}
