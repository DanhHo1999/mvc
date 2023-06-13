using System.ComponentModel.DataAnnotations.Schema;

namespace _06_MvcWeb.Products.Models
{
    [Table("ProductAndCategories")]
    public class ProductsAndCategories
    {
        public int ProductId { set; get; }

        public int CategoryId { set; get; }

        [ForeignKey("ProductId")]
        public Product Product { set; get; }

        [ForeignKey("CategoryId")]
        public ProductCategory ProductCategory { set; get; }
    }

}
