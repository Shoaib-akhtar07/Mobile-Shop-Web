using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MobileShop.Data;
using MobileShop.Models;

namespace MobileShop.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // Apply pending migrations
            await context.Database.MigrateAsync();

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Admin User
            await SeedAdminUserAsync(userManager, configuration);

            // Seed Sample Products
            await SeedProductsAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Customer", "Manager" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            var adminEmail = configuration["AdminSettings:Email"] ?? "admin@mobileshop.com";
            var adminPassword = configuration["AdminSettings:Password"] ?? "ChangeMe@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task SeedProductsAsync(ApplicationDbContext context)
        {
            if (await context.Products.AnyAsync()) return;

            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "iPhone 15 Pro Max",
                    Model = "A2849",
                    Description = "The most advanced iPhone ever with A17 Pro chip, titanium design, and 48MP camera system.",
                    ShortDescription = "A17 Pro chip, 48MP camera, titanium design",
                    OriginalPrice = 159900,
                    SalePrice = 149900,
                    StockQuantity = 50,
                    MainImageUrl = "/images/products/iphone-15-pro-max.jpg",
                    IsFeatured = true,
                    IsNewArrival = true,
                    CategoryId = 1,
                    BrandId = 1,
                    Specifications = new List<ProductSpecification>
                    {
                        new ProductSpecification { Name = "Display", Value = "6.7\" Super Retina XDR OLED", GroupName = "Display" },
                        new ProductSpecification { Name = "Processor", Value = "A17 Pro chip", GroupName = "Performance" },
                        new ProductSpecification { Name = "RAM", Value = "8GB", GroupName = "Performance" },
                        new ProductSpecification { Name = "Storage", Value = "256GB", GroupName = "Storage" },
                        new ProductSpecification { Name = "Rear Camera", Value = "48MP Main + 12MP Ultra Wide + 12MP Telephoto", GroupName = "Camera" },
                        new ProductSpecification { Name = "Front Camera", Value = "12MP TrueDepth", GroupName = "Camera" },
                        new ProductSpecification { Name = "Battery", Value = "4441 mAh", GroupName = "Battery" },
                        new ProductSpecification { Name = "OS", Value = "iOS 17", GroupName = "Software" },
                        new ProductSpecification { Name = "5G", Value = "Yes", GroupName = "Connectivity" },
                        new ProductSpecification { Name = "Weight", Value = "221g", GroupName = "Physical" }
                    }
                },
                new Product
                {
                    Id = 2,
                    Name = "Samsung Galaxy S24 Ultra",
                    Model = "SM-S928B",
                    Description = "Galaxy AI is here. With AI-powered features, 200MP camera, and S Pen functionality.",
                    ShortDescription = "Galaxy AI, 200MP camera, S Pen included",
                    OriginalPrice = 129999,
                    SalePrice = 119999,
                    StockQuantity = 45,
                    MainImageUrl = "/images/products/galaxy-s24-ultra.jpg",
                    IsFeatured = true,
                    IsBestseller = true,
                    CategoryId = 1,
                    BrandId = 2,
                    Specifications = new List<ProductSpecification>
                    {
                        new ProductSpecification { Name = "Display", Value = "6.8\" Dynamic AMOLED 2X", GroupName = "Display" },
                        new ProductSpecification { Name = "Processor", Value = "Snapdragon 8 Gen 3", GroupName = "Performance" },
                        new ProductSpecification { Name = "RAM", Value = "12GB", GroupName = "Performance" },
                        new ProductSpecification { Name = "Storage", Value = "256GB", GroupName = "Storage" },
                        new ProductSpecification { Name = "Rear Camera", Value = "200MP Main + 50MP Telephoto + 12MP Ultra Wide + 10MP Periscope", GroupName = "Camera" },
                        new ProductSpecification { Name = "Front Camera", Value = "12MP", GroupName = "Camera" },
                        new ProductSpecification { Name = "Battery", Value = "5000 mAh", GroupName = "Battery" },
                        new ProductSpecification { Name = "OS", Value = "Android 14 (One UI 6.1)", GroupName = "Software" },
                        new ProductSpecification { Name = "5G", Value = "Yes", GroupName = "Connectivity" },
                        new ProductSpecification { Name = "Weight", Value = "232g", GroupName = "Physical" }
                    }
                },
                new Product
                {
                    Id = 3,
                    Name = "Xiaomi 14 Ultra",
                    Model = "24031PN0DC",
                    Description = "Leica optics, Snapdragon 8 Gen 3, and 5300mAh battery with 90W fast charging.",
                    ShortDescription = "Leica camera, 90W fast charging, flagship performance",
                    OriginalPrice = 99999,
                    SalePrice = 89999,
                    StockQuantity = 30,
                    MainImageUrl = "/images/products/xiaomi-14-ultra.jpg",
                    IsFeatured = true,
                    IsNewArrival = true,
                    CategoryId = 1,
                    BrandId = 3,
                    Specifications = new List<ProductSpecification>
                    {
                        new ProductSpecification { Name = "Display", Value = "6.73\" AMOLED 120Hz", GroupName = "Display" },
                        new ProductSpecification { Name = "Processor", Value = "Snapdragon 8 Gen 3", GroupName = "Performance" },
                        new ProductSpecification { Name = "RAM", Value = "16GB", GroupName = "Performance" },
                        new ProductSpecification { Name = "Storage", Value = "512GB", GroupName = "Storage" },
                        new ProductSpecification { Name = "Rear Camera", Value = "50MP Main + 50MP Telephoto + 50MP Ultra Wide + 50MP Periscope", GroupName = "Camera" },
                        new ProductSpecification { Name = "Front Camera", Value = "32MP", GroupName = "Camera" },
                        new ProductSpecification { Name = "Battery", Value = "5300 mAh", GroupName = "Battery" },
                        new ProductSpecification { Name = "OS", Value = "HyperOS (Android 14)", GroupName = "Software" },
                        new ProductSpecification { Name = "5G", Value = "Yes", GroupName = "Connectivity" },
                        new ProductSpecification { Name = "Weight", Value = "219.8g", GroupName = "Physical" }
                    }
                },
                new Product
                {
                    Id = 4,
                    Name = "OnePlus 12",
                    Model = "CPH2581",
                    Description = "Smooth beyond belief with 120Hz ProXDR display and Hasselblad camera system.",
                    ShortDescription = "Hasselblad camera, 100W charging, 120Hz display",
                    OriginalPrice = 69999,
                    SalePrice = 64999,
                    StockQuantity = 40,
                    MainImageUrl = "/images/products/oneplus-12.jpg",
                    IsBestseller = true,
                    CategoryId = 1,
                    BrandId = 4,
                    Specifications = new List<ProductSpecification>
                    {
                        new ProductSpecification { Name = "Display", Value = "6.82\" LTPO AMOLED 120Hz", GroupName = "Display" },
                        new ProductSpecification { Name = "Processor", Value = "Snapdragon 8 Gen 3", GroupName = "Performance" },
                        new ProductSpecification { Name = "RAM", Value = "12GB", GroupName = "Performance" },
                        new ProductSpecification { Name = "Storage", Value = "256GB", GroupName = "Storage" },
                        new ProductSpecification { Name = "Rear Camera", Value = "50MP Main + 64MP Telephoto + 48MP Ultra Wide", GroupName = "Camera" },
                        new ProductSpecification { Name = "Front Camera", Value = "32MP", GroupName = "Camera" },
                        new ProductSpecification { Name = "Battery", Value = "5400 mAh", GroupName = "Battery" },
                        new ProductSpecification { Name = "OS", Value = "OxygenOS 14 (Android 14)", GroupName = "Software" },
                        new ProductSpecification { Name = "5G", Value = "Yes", GroupName = "Connectivity" },
                        new ProductSpecification { Name = "Weight", Value = "220g", GroupName = "Physical" }
                    }
                },
                new Product
                {
                    Id = 5,
                    Name = "Google Pixel 8 Pro",
                    Model = "GC3VE",
                    Description = "The best of Google with AI-powered features and the best camera on a Pixel.",
                    ShortDescription = "Google AI, best Pixel camera, 7 years updates",
                    OriginalPrice = 106900,
                    SalePrice = 99999,
                    StockQuantity = 25,
                    MainImageUrl = "/images/products/pixel-8-pro.jpg",
                    IsNewArrival = true,
                    CategoryId = 1,
                    BrandId = 5,
                    Specifications = new List<ProductSpecification>
                    {
                        new ProductSpecification { Name = "Display", Value = "6.7\" LTPO OLED 120Hz", GroupName = "Display" },
                        new ProductSpecification { Name = "Processor", Value = "Google Tensor G3", GroupName = "Performance" },
                        new ProductSpecification { Name = "RAM", Value = "12GB", GroupName = "Performance" },
                        new ProductSpecification { Name = "Storage", Value = "128GB", GroupName = "Storage" },
                        new ProductSpecification { Name = "Rear Camera", Value = "50MP Main + 48MP Ultra Wide + 48MP Telephoto", GroupName = "Camera" },
                        new ProductSpecification { Name = "Front Camera", Value = "10.5MP", GroupName = "Camera" },
                        new ProductSpecification { Name = "Battery", Value = "5050 mAh", GroupName = "Battery" },
                        new ProductSpecification { Name = "OS", Value = "Android 14", GroupName = "Software" },
                        new ProductSpecification { Name = "5G", Value = "Yes", GroupName = "Connectivity" },
                        new ProductSpecification { Name = "Weight", Value = "213g", GroupName = "Physical" }
                    }
                },
                new Product
                {
                    Id = 6,
                    Name = "iPhone 15",
                    Model = "A3092",
                    Description = "Dynamic Island, 48MP Main camera, and USB-C. A powerful upgrade.",
                    ShortDescription = "Dynamic Island, 48MP camera, USB-C",
                    OriginalPrice = 79900,
                    SalePrice = 74900,
                    StockQuantity = 60,
                    MainImageUrl = "/images/products/iphone-15.jpg",
                    IsBestseller = true,
                    CategoryId = 1,
                    BrandId = 1,
                    Specifications = new List<ProductSpecification>
                    {
                        new ProductSpecification { Name = "Display", Value = "6.1\" Super Retina XDR OLED", GroupName = "Display" },
                        new ProductSpecification { Name = "Processor", Value = "A16 Bionic", GroupName = "Performance" },
                        new ProductSpecification { Name = "RAM", Value = "6GB", GroupName = "Performance" },
                        new ProductSpecification { Name = "Storage", Value = "128GB", GroupName = "Storage" },
                        new ProductSpecification { Name = "Rear Camera", Value = "48MP Main + 12MP Ultra Wide", GroupName = "Camera" },
                        new ProductSpecification { Name = "Front Camera", Value = "12MP TrueDepth", GroupName = "Camera" },
                        new ProductSpecification { Name = "Battery", Value = "3349 mAh", GroupName = "Battery" },
                        new ProductSpecification { Name = "OS", Value = "iOS 17", GroupName = "Software" },
                        new ProductSpecification { Name = "5G", Value = "Yes", GroupName = "Connectivity" },
                        new ProductSpecification { Name = "Weight", Value = "171g", GroupName = "Physical" }
                    }
                },
                new Product
                {
                    Id = 7,
                    Name = "Samsung Galaxy A54 5G",
                    Model = "SM-A546E",
                    Description = "Awesome screen, awesome camera, awesome durability at a great price.",
                    ShortDescription = "120Hz AMOLED, 50MP camera, IP67 waterproof",
                    OriginalPrice = 38999,
                    SalePrice = 32999,
                    StockQuantity = 80,
                    MainImageUrl = "/images/products/galaxy-a54.jpg",
                    IsFeatured = true,
                    CategoryId = 1,
                    BrandId = 2,
                    Specifications = new List<ProductSpecification>
                    {
                        new ProductSpecification { Name = "Display", Value = "6.4\" Super AMOLED 120Hz", GroupName = "Display" },
                        new ProductSpecification { Name = "Processor", Value = "Exynos 1380", GroupName = "Performance" },
                        new ProductSpecification { Name = "RAM", Value = "8GB", GroupName = "Performance" },
                        new ProductSpecification { Name = "Storage", Value = "128GB", GroupName = "Storage" },
                        new ProductSpecification { Name = "Rear Camera", Value = "50MP Main + 12MP Ultra Wide + 5MP Macro", GroupName = "Camera" },
                        new ProductSpecification { Name = "Front Camera", Value = "32MP", GroupName = "Camera" },
                        new ProductSpecification { Name = "Battery", Value = "5000 mAh", GroupName = "Battery" },
                        new ProductSpecification { Name = "OS", Value = "Android 14 (One UI 6)", GroupName = "Software" },
                        new ProductSpecification { Name = "5G", Value = "Yes", GroupName = "Connectivity" },
                        new ProductSpecification { Name = "Weight", Value = "202g", GroupName = "Physical" }
                    }
                },
                new Product
                {
                    Id = 8,
                    Name = "Realme GT 5 Pro",
                    Model = "RMX3888",
                    Description = "Flagship performance with Snapdragon 8 Gen 3 and 5400mAh battery.",
                    ShortDescription = "Snapdragon 8 Gen 3, 100W charging, periscope camera",
                    OriginalPrice = 49999,
                    SalePrice = 44999,
                    StockQuantity = 35,
                    MainImageUrl = "/images/products/realme-gt5-pro.jpg",
                    IsNewArrival = true,
                    CategoryId = 1,
                    BrandId = 6,
                    Specifications = new List<ProductSpecification>
                    {
                        new ProductSpecification { Name = "Display", Value = "6.78\" AMOLED 144Hz", GroupName = "Display" },
                        new ProductSpecification { Name = "Processor", Value = "Snapdragon 8 Gen 3", GroupName = "Performance" },
                        new ProductSpecification { Name = "RAM", Value = "12GB", GroupName = "Performance" },
                        new ProductSpecification { Name = "Storage", Value = "256GB", GroupName = "Storage" },
                        new ProductSpecification { Name = "Rear Camera", Value = "50MP Main + 50MP Periscope + 8MP Ultra Wide", GroupName = "Camera" },
                        new ProductSpecification { Name = "Front Camera", Value = "32MP", GroupName = "Camera" },
                        new ProductSpecification { Name = "Battery", Value = "5400 mAh", GroupName = "Battery" },
                        new ProductSpecification { Name = "OS", Value = "realme UI 5.0 (Android 14)", GroupName = "Software" },
                        new ProductSpecification { Name = "5G", Value = "Yes", GroupName = "Connectivity" },
                        new ProductSpecification { Name = "Weight", Value = "218g", GroupName = "Physical" }
                    }
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}