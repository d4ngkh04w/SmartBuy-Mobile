using api.Helpers;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Database
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<ProductLine> ProductLines { get; set; } = null!;
        public DbSet<ProductColor> Colors { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserToken> UserTokens { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;
        public DbSet<ProductDiscount> ProductDiscounts { get; set; } = null!;
        public DbSet<Discount> Discounts { get; set; } = null!;
        public DbSet<ProductDetail> ProductDetails { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(u => new { u.Email, u.PhoneNumber })
                .IsUnique();

            // Thiết lập mối quan hệ User - UserToken
            builder.Entity<User>()
                .HasMany(u => u.Tokens)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserToken>()
                .HasIndex(t => t.TokenHash);
            builder.Entity<UserToken>()
                .HasIndex(t => t.ExpiryDate);
            builder.Entity<UserToken>()
                .HasIndex(t => t.TokenType);

            builder.Entity<Order>()
                .HasIndex(o => o.Status);

            // Cấu hình mối quan hệ Brand - ProductLine
            builder.Entity<Brand>()
                .HasMany(b => b.ProductLines)
                .WithOne(pl => pl.Brand)
                .HasForeignKey(pl => pl.BrandId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa thương hiệu sẽ xóa tất cả dòng sản phẩm của nó
            builder.Entity<Brand>()
                .HasIndex(b => b.Name)
                .IsUnique();

            // Cấu hình mối quan hệ ProductLine - Product
            builder.Entity<ProductLine>()
                .HasMany(pl => pl.Products)
                .WithOne(p => p.ProductLine)
                .HasForeignKey(p => p.ProductLineId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ProductLine>()
                .HasIndex(pl => pl.Name)
                .IsUnique();

            // Cấu hình mối quan hệ Product - ProductColor
            builder.Entity<Product>()
                .HasMany(p => p.Colors)
                .WithOne(c => c.Product)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ ProductColor - ProductImage
            builder.Entity<ProductColor>()
                .HasMany(c => c.Images)
                .WithOne(i => i.Color)
                .HasForeignKey(i => i.ColorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ Product - ProductDiscount
            builder.Entity<ProductDiscount>()
                .HasKey(pd => new { pd.ProductId, pd.DiscountId });

            builder.Entity<ProductDiscount>()
                .HasOne(pd => pd.Product)
                .WithMany(p => p.Discounts)
                .HasForeignKey(pd => pd.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductDiscount>()
                .HasOne(pd => pd.Discount)
                .WithMany(d => d.Products)
                .HasForeignKey(pd => pd.DiscountId);
            //     .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ Product - ProductDetail
            builder.Entity<Product>()
                .HasOne(p => p.Detail)
                .WithOne(d => d.Product)
                .HasForeignKey<ProductDetail>(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ User - Cart
            builder.Entity<User>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ Cart - CartItem
            builder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Product>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Product)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Thêm tài khoản admin
            builder.Entity<User>().HasData(
                new User
                {
                    Email = ConfigHelper.Email,
                    EmailConfirmed = true,
                    PhoneNumber = ConfigHelper.AdminPhoneNumber,
                    PhoneNumberConfirmed = true,
                    Password = BCrypt.Net.BCrypt.HashPassword(ConfigHelper.AdminPassword, BCrypt.Net.BCrypt.GenerateSalt(11, 'b')),
                    Name = ConfigHelper.AdminName,
                    Role = "admin",
                    CreatedAt = DateTime.Now,
                }
            );

            base.OnModelCreating(builder);
        }
    }
}