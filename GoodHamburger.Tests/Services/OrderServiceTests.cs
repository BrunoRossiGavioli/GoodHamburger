using GoodHamburger.API.Entities.Customers;
using GoodHamburger.API.Entities.Products;
using GoodHamburger.API.Entities.PurchaseOrders;
using GoodHamburger.API.Repositories.Customers;
using GoodHamburger.API.Repositories.Products;
using GoodHamburger.API.Repositories.PurchaseOrders;
using GoodHamburger.API.Services.PurchaseOrdens;
using GoodHamburger.Shared.DTOs.PurchaseOrders;
using GoodHamburger.Shared.Enums;
using GoodHamburger.Shared.Exceptions.BusinessRules;
using Moq;
using System.Linq.Expressions;

namespace GoodHamburger.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(_orderRepoMock.Object, _customerRepoMock.Object, _productRepoMock.Object);
    }

    // ── Constructor guards ────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullOrderRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new OrderService(null!, _customerRepoMock.Object, _productRepoMock.Object));
    }

    [Fact]
    public void Constructor_NullCustomerRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new OrderService(_orderRepoMock.Object, null!, _productRepoMock.Object));
    }

    [Fact]
    public void Constructor_NullProductRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new OrderService(_orderRepoMock.Object, _customerRepoMock.Object, null!));
    }

    // ── GetAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_ExistingOrder_ReturnsOrder()
    {
        var id = Guid.NewGuid();
        var entity = BuildAnonymousOrderEntity(id);
        _orderRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var result = await _sut.GetAsync(new GetOrderDto(id));

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task GetAsync_NonExistingOrder_ReturnsNull()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrderEntity?)null);

        var result = await _sut.GetAsync(new GetOrderDto(Guid.NewGuid()));

        Assert.Null(result);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrdersMapped()
    {
        var entities = new List<OrderEntity> { BuildAnonymousOrderEntity(), BuildAnonymousOrderEntity() };
        _orderRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    // ── FindAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task FindAsync_ByCustomerId_ReturnsMappedOrders()
    {
        var entities = new List<OrderEntity> { BuildAnonymousOrderEntity() };
        _orderRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrderEntity, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entities);

        var result = await _sut.FindAsync(new FindOrderDto(CustomerId: Guid.NewGuid()));

        Assert.Single(result);
    }

    [Fact]
    public async Task FindAsync_ByOrderDate_ReturnsMappedOrders()
    {
        var entities = new List<OrderEntity> { BuildAnonymousOrderEntity() };
        _orderRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrderEntity, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entities);

        var result = await _sut.FindAsync(new FindOrderDto(OrderDate: DateTime.UtcNow.Date));

        Assert.Single(result);
    }

    // ── UpdateStatus ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatus_ExistingOrder_UpdatesStatus()
    {
        var id = Guid.NewGuid();
        var entity = BuildAnonymousOrderEntity(id);
        _orderRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _orderRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.UpdateStatus(new UpdateOrderStatusDto(id, OrderStatus.Confirmado));

        Assert.Equal(OrderStatus.Confirmado, entity.Status);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_OrderNotFound_ThrowsInvalidOperationException()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrderEntity?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.UpdateStatus(new UpdateOrderStatusDto(Guid.NewGuid(), OrderStatus.Cancelado)));
    }

    // ── CreateAsync — customer validation ─────────────────────────────────────

    [Fact]
    public async Task CreateAsync_RegisteredCustomer_NotFound_ThrowsInvalidOperationException()
    {
        _customerRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CustomerEntity?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(new CreateOrderDto(Guid.NewGuid(), null!, null!, null!, [])));
    }

    [Fact]
    public async Task CreateAsync_RegisteredCustomer_Exists_ReturnsOrderWithCustomerData()
    {
        var customerId = Guid.NewGuid();
        var customer = BuildCustomerEntity(customerId);
        var sandwich = BuildProductEntity(type: ProductType.Sandwich, price: 5.00m);

        SetupBasicMocks(customerEntity: customer, products: [sandwich]);

        var dto = new CreateOrderDto(
            CustomerId: customerId,
            CustomerName: null!, CustomerPhone: null!, CustomerAddress: null!,
            items: [new CreateOrderItemDto(sandwich.Id, 1, sandwich.Prices.First().Value, "")]);

        var result = await _sut.CreateAsync(dto);

        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal("Ana Silva", result.CustomerName);
    }

    // ── CreateAsync — product validation ──────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ProductNotFound_ThrowsInvalidOperationException()
    {
        SetupBasicMocks(products: []);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
                [new CreateOrderItemDto(Guid.NewGuid(), 1, 5.00m, "")])));
    }

    // ── CreateAsync — business rules (ThrowIfInvalidOrder) ────────────────────

    [Fact]
    public async Task CreateAsync_QuantityMoreThanOne_ThrowsOrderException()
    {
        var sandwich = BuildProductEntity(type: ProductType.Sandwich, price: 5.00m);
        SetupBasicMocks(products: [sandwich]);

        await Assert.ThrowsAsync<OrderException>(() =>
            _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
                [new CreateOrderItemDto(sandwich.Id, 2, 5.00m, "")])));
    }

    [Fact]
    public async Task CreateAsync_DuplicateSandwich_ThrowsOrderException()
    {
        var sandwich1 = BuildProductEntity(type: ProductType.Sandwich, price: 5.00m);
        var sandwich2 = BuildProductEntity(type: ProductType.Sandwich, price: 7.00m);
        SetupBasicMocks(products: [sandwich1, sandwich2]);

        await Assert.ThrowsAsync<OrderException>(() =>
            _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
            [
                new CreateOrderItemDto(sandwich1.Id, 1, 5.00m, ""),
                new CreateOrderItemDto(sandwich2.Id, 1, 7.00m, "")
            ])));
    }

    [Fact]
    public async Task CreateAsync_DuplicateDrink_ThrowsOrderException()
    {
        var drink1 = BuildProductEntity(type: ProductType.Drink, price: 2.50m);
        var drink2 = BuildProductEntity(type: ProductType.Drink, price: 2.50m);
        SetupBasicMocks(products: [drink1, drink2]);

        await Assert.ThrowsAsync<OrderException>(() =>
            _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
            [
                new CreateOrderItemDto(drink1.Id, 1, 2.50m, ""),
                new CreateOrderItemDto(drink2.Id, 1, 2.50m, "")
            ])));
    }

    // ── CreateAsync — discount rules (CalculateSubtotalAndDiscount) ───────────

    [Fact]
    public async Task CreateAsync_SandwichPlusDrinkPlusFries_Applies20PercentDiscount()
    {
        var sandwich = BuildProductEntity(type: ProductType.Sandwich, price: 5.00m);
        var drink = BuildProductEntity(type: ProductType.Drink, price: 2.50m);
        var fries = BuildProductEntity(type: ProductType.Fries, price: 2.00m);
        SetupBasicMocks(products: [sandwich, drink, fries]);

        var result = await _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
        [
            new CreateOrderItemDto(sandwich.Id, 1, 5.00m, ""),
            new CreateOrderItemDto(drink.Id,    1, 2.50m, ""),
            new CreateOrderItemDto(fries.Id,    1, 2.00m, "")
        ]));

        Assert.Equal(9.50m, result.Subtotal);
        Assert.Equal(1.90m, result.Discount);
        Assert.Equal(7.60m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_SandwichPlusDrink_Applies15PercentDiscount()
    {
        var sandwich = BuildProductEntity(type: ProductType.Sandwich, price: 5.00m);
        var drink = BuildProductEntity(type: ProductType.Drink, price: 2.50m);
        SetupBasicMocks(products: [sandwich, drink]);

        var result = await _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
        [
            new CreateOrderItemDto(sandwich.Id, 1, 5.00m, ""),
            new CreateOrderItemDto(drink.Id,    1, 2.50m, "")
        ]));

        Assert.Equal(7.50m, result.Subtotal);
        Assert.Equal(1.125m, result.Discount);
        Assert.Equal(6.375m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_SandwichPlusFries_Applies10PercentDiscount()
    {
        var sandwich = BuildProductEntity(type: ProductType.Sandwich, price: 5.00m);
        var fries = BuildProductEntity(type: ProductType.Fries, price: 2.00m);
        SetupBasicMocks(products: [sandwich, fries]);

        var result = await _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
        [
            new CreateOrderItemDto(sandwich.Id, 1, 5.00m, ""),
            new CreateOrderItemDto(fries.Id,    1, 2.00m, "")
        ]));

        Assert.Equal(7.00m, result.Subtotal);
        Assert.Equal(0.70m, result.Discount);
        Assert.Equal(6.30m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_SandwichOnly_NoDiscount()
    {
        var sandwich = BuildProductEntity(type: ProductType.Sandwich, price: 5.00m);
        SetupBasicMocks(products: [sandwich]);

        var result = await _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
            [new CreateOrderItemDto(sandwich.Id, 1, 5.00m, "")]));

        Assert.Equal(5.00m, result.Subtotal);
        Assert.Equal(0m, result.Discount);
        Assert.Equal(5.00m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_FriesOnly_NoDiscount()
    {
        var fries = BuildProductEntity(type: ProductType.Fries, price: 2.00m);
        SetupBasicMocks(products: [fries]);

        var result = await _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
            [new CreateOrderItemDto(fries.Id, 1, 2.00m, "")]));

        Assert.Equal(2.00m, result.Subtotal);
        Assert.Equal(0m, result.Discount);
        Assert.Equal(2.00m, result.Total);
    }

    // ── CreateAsync — anonymous customer fields ────────────────────────────────

    [Fact]
    public async Task CreateAsync_AnonymousCustomer_TrimsAndSetsFields()
    {
        var sandwich = BuildProductEntity(type: ProductType.Sandwich, price: 5.00m);
        SetupBasicMocks(products: [sandwich]);

        var result = await _sut.CreateAsync(new CreateOrderDto(
            null, " João ", " 11999999999 ", " Rua A, 1 ",
            [new CreateOrderItemDto(sandwich.Id, 1, 5.00m, "")]));

        Assert.Null(result.CustomerId);
        Assert.Equal("João", result.CustomerName);
        Assert.Equal("11999999999", result.CustomerPhone);
        Assert.Equal("Rua A, 1", result.CustomerAddress);
    }

    [Fact]
    public async Task CreateAsync_AnonymousCustomer_SetsOrderDateToUtcNow()
    {
        var before = DateTime.UtcNow;
        var sandwich = BuildProductEntity(type: ProductType.Sandwich, price: 5.00m);
        SetupBasicMocks(products: [sandwich]);

        var result = await _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
            [new CreateOrderItemDto(sandwich.Id, 1, 5.00m, "")]));

        Assert.True(result.OrderDate >= before);
    }

    [Fact]
    public async Task CreateAsync_AnonymousCustomer_ItemObservationTrimmed()
    {
        var sandwich = BuildProductEntity(type: ProductType.Sandwich, price: 5.00m);
        SetupBasicMocks(products: [sandwich]);

        var result = await _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
            [new CreateOrderItemDto(sandwich.Id, 1, 5.00m, " sem cebola ")]));

        Assert.Equal("sem cebola", result.Items.Single().Observation);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetupBasicMocks(CustomerEntity? customerEntity = null, List<ProductEntity>? products = null)
    {
        if (customerEntity is not null)
            _customerRepoMock.Setup(r => r.GetByIdAsync(customerEntity.Id, It.IsAny<CancellationToken>()))
                             .ReturnsAsync(customerEntity);

        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ProductEntity, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(products ?? []);

        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((OrderEntity e, CancellationToken _) => e);
        _orderRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static OrderEntity BuildAnonymousOrderEntity(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        CustomerId = null,
        CustomerName = "João",
        CustomerPhone = "11999999999",
        CustomerAddress = "Rua A, 1",
        OrderDate = DateTime.UtcNow,
        Subtotal = 5.00m,
        Discount = 0m,
        Total = 5.00m,
        Status = OrderStatus.Pendente,
        Items = []
    };

    private static CustomerEntity BuildCustomerEntity(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = "Ana Silva",
        Phone = "11988888888",
        Address = "Rua B, 2",
        Orders = []
    };

    private static ProductEntity BuildProductEntity(ProductType type, decimal price)
    {
        var id = Guid.NewGuid();
        return new ProductEntity
        {
            Id = id,
            Name = type.ToString(),
            Description = "",
            Type = type,
            IsActive = true,
            Prices =
            [
                new ProductPriceEntity
                {
                    Id = Guid.NewGuid(),
                    ProductId = id,
                    Value = price,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = null,
                    Reason = "Inicial"
                }
            ]
        };
    }
}
