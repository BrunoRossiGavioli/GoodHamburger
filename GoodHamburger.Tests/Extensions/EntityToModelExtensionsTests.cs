using GoodHamburger.API.Entities.Customers;
using GoodHamburger.API.Entities.Products;
using GoodHamburger.API.Entities.PurchaseOrders;
using GoodHamburger.API.Extensions.Entities;
using GoodHamburger.Shared.Enums;

namespace GoodHamburger.Tests.Extensions;

public class EntityToModelExtensionsTests
{
    // ── CustomerEntity → Customer ─────────────────────────────────────────────

    [Fact]
    public void MapCustomer_MapsAllFields()
    {
        var id = Guid.NewGuid();
        var entity = new CustomerEntity { Id = id, Name = "Bruno", Phone = "11999999999", Address = "Rua A, 1" };

        var model = entity.MapEntityToModel();

        Assert.Equal(id, model.Id);
        Assert.Equal("Bruno", model.Name);
        Assert.Equal("11999999999", model.Phone);
        Assert.Equal("Rua A, 1", model.Address);
    }

    // ── ProductPriceEntity → ProductPrice ─────────────────────────────────────

    [Fact]
    public void MapProductPrice_MapsAllFields()
    {
        var id = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2025, 12, 31);
        var entity = new ProductPriceEntity
        {
            Id = id, ProductId = productId, Value = 5.00m,
            StartDate = start, EndDate = end, Reason = "Inicial"
        };

        var model = entity.MapEntityToModel();

        Assert.Equal(id, model.Id);
        Assert.Equal(productId, model.ProductId);
        Assert.Equal(5.00m, model.Value);
        Assert.Equal(start, model.StartDate);
        Assert.Equal(end, model.EndDate);
        Assert.Equal("Inicial", model.Reason);
    }

    // ── ProductEntity → Product ───────────────────────────────────────────────

    [Fact]
    public void MapProduct_MapsAllFieldsWithCurrentPrice()
    {
        var id = Guid.NewGuid();
        var entity = BuildProduct(id, price: 7.00m, type: ProductType.Sandwich);

        var model = entity.MapEntityToModel();

        Assert.Equal(id, model.Id);
        Assert.Equal("X Bacon", model.Name);
        Assert.Equal(7.00m, model.Price);
        Assert.Equal(ProductType.Sandwich, model.Type);
    }

    [Fact]
    public void MapProduct_WithDateRef_UsesHistoricalPrice()
    {
        var id = Guid.NewGuid();
        var dateRef = new DateTime(2024, 6, 1);
        var entity = new ProductEntity
        {
            Id = id,
            Name = "X Egg",
            Description = "Com ovo",
            Type = ProductType.Sandwich,
            IsActive = true,
            Prices =
            [
                new ProductPriceEntity { Id = Guid.NewGuid(), ProductId = id, Value = 3.50m, StartDate = new DateTime(2023, 1, 1), EndDate = new DateTime(2024, 12, 31), Reason = "Antigo" },
                new ProductPriceEntity { Id = Guid.NewGuid(), ProductId = id, Value = 4.50m, StartDate = new DateTime(2025, 1, 1), EndDate = null, Reason = "Atual" }
            ]
        };

        var model = entity.MapEntityToModel(dateRef);

        Assert.Equal(3.50m, model.Price);
    }

    // ── OrderEntity → Order (anonymous customer) ──────────────────────────────

    [Fact]
    public void MapOrder_AnonymousCustomer_MapsCustomerFields()
    {
        var id = Guid.NewGuid();
        var entity = new OrderEntity
        {
            Id = id,
            CustomerId = null,
            CustomerName = "João",
            CustomerPhone = "11999999999",
            CustomerAddress = "Rua B, 2",
            OrderDate = new DateTime(2025, 6, 1),
            Subtotal = 9.50m,
            Discount = 0m,
            Total = 9.50m,
            Status = OrderStatus.Pendente,
            Items = []
        };

        var model = entity.MapEntityToModel();

        Assert.Equal(id, model.Id);
        Assert.Null(model.CustomerId);
        Assert.Equal("João", model.CustomerName);
        Assert.Equal("11999999999", model.CustomerPhone);
        Assert.Equal("Rua B, 2", model.CustomerAddress);
        Assert.Equal(9.50m, model.Subtotal);
        Assert.Equal(0m, model.Discount);
        Assert.Equal(9.50m, model.Total);
        Assert.Empty(model.Items);
    }

    [Fact]
    public void MapOrder_AnonymousCustomer_MapsItemsWithProductPrice()
    {
        var productId = Guid.NewGuid();
        var orderDate = DateTime.UtcNow;
        var product = BuildProduct(productId, name: "Batata Frita", price: 2.00m, type: ProductType.Fries);

        var entity = new OrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = null,
            CustomerName = "Maria",
            CustomerPhone = "11988888888",
            CustomerAddress = "Rua C",
            OrderDate = orderDate,
            Subtotal = 2.00m, Discount = 0m, Total = 2.00m,
            Status = OrderStatus.Pendente,
            Items =
            [
                new OrderItemEntity
                {
                    ProductId = productId,
                    Quantity = 1,
                    Observation = "Sem sal",
                    Product = product,
                    Order = null!
                }
            ]
        };

        var model = entity.MapEntityToModel();

        Assert.Single(model.Items);
        var item = model.Items.First();
        Assert.Equal(1, item.Quantity);
        Assert.Equal("Sem sal", item.Observation);
        Assert.Equal("Batata Frita", item.Product.Name);
        Assert.Equal(2.00m, item.Product.Price);
    }

    // ── OrderEntity → Order (registered customer) ─────────────────────────────

    [Fact]
    public void MapOrder_RegisteredCustomer_MapsFromCustomerNavigation()
    {
        var customerId = Guid.NewGuid();
        var customer = new CustomerEntity { Id = customerId, Name = "Ana", Phone = "11977777777", Address = "Rua D" };
        var entity = new OrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Customer = customer,
            OrderDate = DateTime.UtcNow,
            Subtotal = 5.00m, Discount = 0m, Total = 5.00m,
            Status = OrderStatus.Confirmado,
            Items = []
        };

        var model = entity.MapEntityToModel();

        Assert.Equal(customerId, model.CustomerId);
        Assert.Equal("Ana", model.CustomerName);
        Assert.Equal("11977777777", model.CustomerPhone);
        Assert.Equal("Rua D", model.CustomerAddress);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ProductEntity BuildProduct(Guid? id = null, string name = "X Bacon", decimal price = 7.00m, ProductType type = ProductType.Sandwich)
    {
        var productId = id ?? Guid.NewGuid();
        return new ProductEntity
        {
            Id = productId,
            Name = name,
            Description = "Teste",
            Type = type,
            IsActive = true,
            Prices =
            [
                new ProductPriceEntity
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Value = price,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = null,
                    Reason = "Inicial"
                }
            ]
        };
    }
}
