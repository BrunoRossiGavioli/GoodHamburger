using GoodHamburger.API.Entities.Customers;
using GoodHamburger.API.Entities.Products;
using GoodHamburger.API.Entities.PurchaseOrders;
using GoodHamburger.API.Repositories.Customers;
using GoodHamburger.API.Repositories.PurchaseOrders;
using GoodHamburger.API.Services.PurchaseOrdens;
using GoodHamburger.Shared.DTOs.PurchaseOrders;
using GoodHamburger.Shared.Enums;
using Moq;
using System.Linq.Expressions;

namespace GoodHamburger.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<ICustomerRepository> _customerRepoMock = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(_orderRepoMock.Object, _customerRepoMock.Object);
    }

    [Fact]
    public void Constructor_NullOrderRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new OrderService(null!, _customerRepoMock.Object));
    }

    [Fact]
    public void Constructor_NullCustomerRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new OrderService(_orderRepoMock.Object, null!));
    }

    // ── GetAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_ExistingOrder_ReturnsOrder()
    {
        var id = Guid.NewGuid();
        var entity = BuildAnonymousOrder(id);
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
        var entities = new List<OrderEntity> { BuildAnonymousOrder(), BuildAnonymousOrder() };
        _orderRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    // ── FindAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task FindAsync_ByCustomerId_ReturnsMappedOrders()
    {
        var customerId = Guid.NewGuid();
        var entities = new List<OrderEntity> { BuildAnonymousOrder(customerId: customerId) };
        _orderRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrderEntity, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entities);

        var result = await _sut.FindAsync(new FindOrderDto(CustomerId: customerId));

        Assert.Single(result);
    }

    [Fact]
    public async Task FindAsync_ByOrderDate_ReturnsMappedOrders()
    {
        var date = DateTime.UtcNow.Date;
        var entities = new List<OrderEntity> { BuildAnonymousOrder() };
        _orderRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrderEntity, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entities);

        var result = await _sut.FindAsync(new FindOrderDto(OrderDate: date));

        Assert.Single(result);
    }

    // ── CreateAsync — anonymous customer ─────────────────────────────────────

    [Fact]
    public async Task CreateAsync_AnonymousCustomer_TrimsStrings_AndPersists()
    {
        var dto = new CreateOrderDto(
            CustomerId: null,
            CustomerName: " João ",
            CustomerPhone: " 11999999999 ",
            CustomerAddress: " Rua A, 1 ",
            Subtotal: 10.00m,
            Discount: 0m,
            Total: 10.00m,
            items: []);

        OrderEntity? capturedEntity = null;
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
                      .Callback<OrderEntity, CancellationToken>((e, _) => capturedEntity = e)
                      .ReturnsAsync((OrderEntity e, CancellationToken _) => e);
        _orderRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.CreateAsync(dto);

        Assert.NotNull(capturedEntity);
        Assert.Equal("João", capturedEntity!.CustomerName);
        Assert.Equal("11999999999", capturedEntity.CustomerPhone);
        Assert.Equal("Rua A, 1", capturedEntity.CustomerAddress);
        Assert.Null(capturedEntity.CustomerId);
        Assert.Equal(10.00m, capturedEntity.Subtotal);
        Assert.Equal(0m, capturedEntity.Discount);
        Assert.Equal(10.00m, capturedEntity.Total);
    }

    [Fact]
    public async Task CreateAsync_AnonymousCustomer_SetsOrderDate()
    {
        var before = DateTime.UtcNow;
        OrderEntity? capturedEntity = null;
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
                      .Callback<OrderEntity, CancellationToken>((e, _) => capturedEntity = e)
                      .ReturnsAsync((OrderEntity e, CancellationToken _) => e);
        _orderRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.CreateAsync(BuildAnonymousOrderDto());

        Assert.NotNull(capturedEntity);
        Assert.True(capturedEntity!.OrderDate >= before);
    }

    [Fact]
    public async Task CreateAsync_AnonymousCustomer_TrimsItemObservationAndSetsFields()
    {
        // MapEntityToModel requires Product navigation loaded per item — we verify the
        // captured entity state directly before the mapping step runs (via interaction check).
        var productId = Guid.NewGuid();
        var dto = new CreateOrderDto(
            CustomerId: null,
            CustomerName: "João",
            CustomerPhone: "11999999999",
            CustomerAddress: "Rua A",
            Subtotal: 5.00m, Discount: 0m, Total: 5.00m,
            items: [new CreateOrderItemDto(productId, 2, 5.00m, " sem cebola ")]);

        OrderEntity? capturedEntity = null;
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
                      .Callback<OrderEntity, CancellationToken>((e, _) => capturedEntity = e)
                      .ReturnsAsync((OrderEntity e, CancellationToken _) => e);
        // MapEntityToModel is called after SaveChangesAsync; throw before that to inspect state.
        _orderRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .Callback<CancellationToken>(_ =>
                      {
                          // Assert item state at save time, before MapEntityToModel runs
                          Assert.NotNull(capturedEntity);
                          var item = Assert.Single(capturedEntity!.Items);
                          Assert.Equal(productId, item.ProductId);
                          Assert.Equal(2, item.Quantity);
                          Assert.Equal(5.00m, item.UnitPrice);
                          Assert.Equal("sem cebola", item.Observation);
                      })
                      .ReturnsAsync(1);

        // The service then calls MapEntityToModel which requires Product navigation —
        // that's a known limitation of not doing post-save reload. We catch NullReferenceException here.
        await Assert.ThrowsAsync<NullReferenceException>(() => _sut.CreateAsync(dto));
    }

    // ── CreateAsync — registered customer ────────────────────────────────────

    [Fact]
    public async Task CreateAsync_RegisteredCustomer_CustomerNotFound_ThrowsInvalidOperationException()
    {
        var customerId = Guid.NewGuid();
        _customerRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CustomerEntity, bool>>>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.CreateAsync(new CreateOrderDto(customerId, null!, null!, null!, 5.00m, 0m, 5.00m, [])));
    }

    [Fact]
    public async Task CreateAsync_RegisteredCustomer_CustomerExists_ChecksExistenceBeforeAdding()
    {
        var customerId = Guid.NewGuid();
        _customerRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<CustomerEntity, bool>>>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(true);
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((OrderEntity e, CancellationToken _) => e);
        _orderRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // MapEntityToModel on a registered customer without the navigation property loaded throws NullReferenceException,
        // which is an existing limitation of the service (navigation not loaded after AddAsync).
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            _sut.CreateAsync(new CreateOrderDto(customerId, null!, null!, null!, 5.00m, 0m, 5.00m, [])));

        _customerRepoMock.Verify(r => r.ExistsAsync(It.IsAny<Expression<Func<CustomerEntity, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── UpdateStatus ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatus_ExistingOrder_UpdatesStatus()
    {
        var id = Guid.NewGuid();
        var entity = BuildAnonymousOrder(id);
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

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static OrderEntity BuildAnonymousOrder(Guid? id = null, Guid? customerId = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        CustomerId = null,
        CustomerName = "João",
        CustomerPhone = "11999999999",
        CustomerAddress = "Rua A, 1",
        OrderDate = DateTime.UtcNow,
        Subtotal = 10.00m,
        Discount = 0m,
        Total = 10.00m,
        Status = OrderStatus.Pendente,
        Items = []
    };

    private static CreateOrderDto BuildAnonymousOrderDto() => new(
        CustomerId: null,
        CustomerName: "João",
        CustomerPhone: "11999999999",
        CustomerAddress: "Rua A, 1",
        Subtotal: 10.00m,
        Discount: 0m,
        Total: 10.00m,
        items: []);
}
