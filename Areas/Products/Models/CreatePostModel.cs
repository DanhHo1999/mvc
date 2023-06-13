using System.ComponentModel.DataAnnotations;

namespace _06_MvcWeb.Products.Models
{
    public class CreateProductModel:Product
    {
        [Display(Name ="Chuyên mục")]
        public int[] CategoryIds { get; set; }
    }
}