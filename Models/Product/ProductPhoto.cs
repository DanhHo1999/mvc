using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _06_MvcWeb.Products.Models
{
    [Table("ProductPhotos")]
    public class ProductPhoto
    {
        [Key]
        public int PhotoId { get; set; }
        //abc.png, 123.jpg...
        // /contents/Products/abc.png
        public string FileName { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
