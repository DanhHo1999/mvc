using _06_MvcWeb.Models;
using Bogus.DataSets;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _06_MvcWeb.Products.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductId { set; get; }

        [Required(ErrorMessage = "Phải có tiêu đề sản phẩm")]
        [Display(Name = "Tiêu đề")]
        [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} dài {1} đến {2}")]
        public string Title { set; get; }

        [Display(Name = "Mô tả ngắn")]
        public string Description { set; get; }

        [Display(Name = "Chuỗi định danh (url)", Prompt = "Nhập hoặc để trống tự phát sinh theo Title")]
        [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} dài {1} đến {2}")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
        public string? Slug { set; get; }

        [Display(Name = "Nội dung")]
        public string Content { set; get; }

        [Display(Name = "Đã đăng")]
        public bool Published { set; get; }

        [Required]
        [Display(Name = "Người đăng")]
        public string AuthorId { set; get; }

        [ForeignKey("AuthorId")]
        [Display(Name = "Người đăng")]
        public AppUser Author { set; get; }

        [Display(Name = "Ngày tạo")]
        public DateTime DateCreated { set; get; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime DateUpdated { set; get; }

        [Display(Name="Giá sản phẩm")]
        [Range(0,int.MaxValue,ErrorMessage ="Phải nhập giá trị từ {1}")]
        public decimal Price { get; set; }


		public List<ProductsAndCategories> ProductsAndCategories { get; set; }

        public List<ProductPhoto> Photos { get; set; }

	}
}
