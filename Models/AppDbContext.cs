using _06_MvcWeb.Contacts.Models;
using _06_MvcWeb.Blog.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using _06_MvcWeb.Products.Models;

namespace _06_MvcWeb.Models
{
    public class AppDbContext:IdentityDbContext<AppUser>
    {
		public DbSet<Contact> Contacts { get; set; }

		public DbSet<Post> Posts { get; set; }
		public DbSet<PostsAndCategories> PostsAndCategories { get; set; }
		public DbSet<PostCategory> PostCategories { get; set; }

		public DbSet<Product> Products{ get; set; }
		public DbSet<ProductCategory> ProductCategories { get; set; }
		public DbSet<ProductsAndCategories> ProductsAndCategories { get; set; }
        public DbSet<ProductPhoto> ProductPhotos { get; set; }

		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    tableName = tableName.Substring(6);
                    entityType.SetTableName(tableName);
                }
            }
            modelBuilder.Entity<PostCategory>(entity =>
            {
                entity.HasIndex(c => c.Slug).IsUnique();
            });
            modelBuilder.Entity<PostsAndCategories>(entity =>
            {
                entity.HasKey(c => new { c.PostId, c.CategoryId });
            });
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasIndex(c => c.Slug).IsUnique();
            });
			modelBuilder.Entity<ProductCategory>(entity =>
			{
				entity.HasIndex(c => c.Slug).IsUnique();
			});
			modelBuilder.Entity<ProductsAndCategories>(entity =>
			{
				entity.HasKey(c => new { c.ProductId, c.CategoryId });
			});
			modelBuilder.Entity<Product>(entity =>
			{
				entity.HasIndex(c => c.Slug).IsUnique();
			});
		}
    }
}
