
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MobileShop.Models;

namespace MobileShop.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(p => p.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => p.Name);
                entity.HasIndex(p => p.IsFeatured);
                entity.HasIndex(p => p.IsNewArrival);
                entity.HasIndex(p => p.IsBestseller);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(o => o.OrderNumber).IsUnique();
                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.OrderDate);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasIndex(r => r.ProductId);
                entity.HasIndex(r => r.UserId);
            });

            modelBuilder.Entity<ShoppingCartItem>(entity =>
            {
                entity.HasIndex(sci => sci.CartId);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Smartphones", Description = "Latest smartphones from top brands", DisplayOrder = 1, ImageUrl = "/images/categories/smartphones.jpg", IsActive = true, CreatedAt = new DateTime(2026, 5, 3, 21, 56, 5) },
                new Category { Id = 2, Name = "Feature Phones", Description = "Basic phones with essential features", DisplayOrder = 2, ImageUrl = "/images/categories/feature-phones.jpg", IsActive = true, CreatedAt = new DateTime(2026, 5, 3, 21, 56, 5) },
                new Category { Id = 3, Name = "Accessories", Description = "Phone cases, chargers, and more", DisplayOrder = 3, ImageUrl = "/images/categories/accessories.jpg", IsActive = true, CreatedAt = new DateTime(2026, 5, 3, 21, 56, 5) },
                new Category { Id = 4, Name = "Tablets", Description = "Tablets and iPads", DisplayOrder = 4, ImageUrl = "/images/categories/tablets.jpg", IsActive = true, CreatedAt = new DateTime(2026, 5, 3, 21, 56, 5) }
            );

            // Seed Brands
            modelBuilder.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "Apple", Description = "Premium smartphones and devices", Country = "USA", WebsiteUrl = "https://www.apple.com", IsActive = true, CreatedAt = new DateTime(2026, 5, 3, 21, 56, 5) },
                new Brand { Id = 2, Name = "Samsung", Description = "Innovative technology solutions", Country = "South Korea", WebsiteUrl = "https://www.samsung.com", IsActive = true, CreatedAt = new DateTime(2026, 5, 3, 21, 56, 5) },
                new Brand { Id = 3, Name = "Xiaomi", Description = "High-quality at affordable prices", Country = "China", WebsiteUrl = "https://www.mi.com", IsActive = true, CreatedAt = new DateTime(2026, 5, 3, 21, 56, 5) },
                new Brand { Id = 4, Name = "OnePlus", Description = "Flagship killer smartphones", Country = "China", WebsiteUrl = "https://www.oneplus.com", IsActive = true, CreatedAt = new DateTime(2026, 5, 3, 21, 56, 5) },
                new Brand { Id = 5, Name = "Google", Description = "Pixel smartphones by Google", Country = "USA", WebsiteUrl = "https://store.google.com", IsActive = true, CreatedAt = new DateTime(2026, 5, 3, 21, 56, 5) },
                new Brand { Id = 6, Name = "Realme", Description = "Dare to leap", Country = "China", WebsiteUrl = "https://www.realme.com", IsActive = true, CreatedAt = new DateTime(2026, 5, 3, 21, 56, 5) },
                new Brand { Id = 7, Name = "Oppo", Description = "Camera phone specialists", Country = "China", WebsiteUrl = "https://www.oppo.com", IsActive = true, CreatedAt = new DateTime(2026, 5, 3, 21, 56, 5) },
                new Brand { Id = 8, Name = "Vivo", Description = "Camera and music phones", Country = "China", WebsiteUrl = "https://www.vivo.com", IsActive = true, CreatedAt = new DateTime(2026, 5, 3, 21, 56, 5) }
            );
        }
    }
}
