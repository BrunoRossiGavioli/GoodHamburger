using GoodHamburger.API.Entities.Auth;
using GoodHamburger.API.Entities.Customers;
using GoodHamburger.API.Entities.Products;
using GoodHamburger.API.Entities.PurchaseOrders;
using GoodHamburger.API.Extensions.Data;
using GoodHamburger.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GoodHamburger.API.Data;

public class AppDbContext : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>
{
    public DbSet<CustomerEntity> Customers { get; set; }
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<ProductPriceEntity> ProductPrices { get; set; }
    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderItemEntity> OrderItems { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.UseSqliteCaseInsensitiveCollation();

        builder.Entity<UserEntity>(entity =>
        {
            entity.Property(u => u.Name).HasMaxLength(100).IsRequired();
            entity.Property(u => u.IsActive).HasDefaultValue(true);
        });

        var productTypeConverter = new ValueConverter<ProductType, int>(
            v => (int)v,
            v => (ProductType)v);

        builder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(400);

            entity.Property(e => e.Type)
                .IsRequired()
                .HasConversion(productTypeConverter);

            entity.HasMany(e => e.Prices)
                .WithOne(e => e.Product)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductPriceEntity>(entity =>
        {
            entity.ToTable("ProductPrices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.ProductId).IsRequired();

            entity.Property(e => e.Value)
                .IsRequired()
                .HasPrecision(10, 2);

            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired(false);

            entity.Property(e => e.Reason)
                .HasMaxLength(200)
                .IsRequired(false);

            entity.HasIndex(e => new { e.ProductId, e.StartDate }).IsUnique();
        });

        builder.Entity<CustomerEntity>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(e => e.Phone).IsUnique();

            entity.HasMany(e => e.Orders)
                .WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.OrderDate)
                .IsRequired();

            entity.Property(e => e.CustomerId).IsRequired(false);

            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CustomerPhone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.CustomerAddress)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Subtotal)
                .IsRequired()
                .HasPrecision(10, 2);

            entity.Property(e => e.Discount)
                .IsRequired()
                .HasPrecision(10, 2)
                .HasDefaultValue(0);

            entity.Property(e => e.Total)
                .IsRequired()
                .HasPrecision(10, 2);

            entity.HasMany(e => e.Items)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<OrderItemEntity>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(e => new { e.OrderId, e.ProductId });

            entity.Property(e => e.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            entity.Property(e => e.Observation)
                .HasMaxLength(200)
                .IsRequired(false);

            entity.HasOne(e => e.Order)
                .WithMany(e => e.Items)
                .HasForeignKey(e => e.OrderId);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
