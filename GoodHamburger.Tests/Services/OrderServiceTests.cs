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

        var result = await _sut.FindAsync(new FindOrderDto(OrderDate: Datetime.Now.Date));

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

        await _sut.UpdateStatus(new UpdateOrderStatusDto(id, OrderStatus.Confirmed));

        Assert.Equal(OrderStatus.Confirmed, entity.Status);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_OrderNotFound_ThrowsInvalidOperationException()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((OrderEntity?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.UpdateStatus(new UpdateOrderStatusDto(Guid.NewGuid(), OrderStatus.Cancelled)));
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
            items: [new CreateOrderItemDto(sandwich.Id, 1, "")]);

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
                [new CreateOrderItemDto(Guid.NewGuid(), 1, "")])));
    }

    // ── CreateAsync — business rules (ThrowIfInvalidOrder) ────────────────────

    [Fact]
    public async Task CreateAsync_QuantityMoreThanOne_ThrowsOrderException()
    {
        var sandwich = BuildProductEntity(type: ProductType.Sandwich, price: 5.00m);
        SetupBasicMocks(products: [sandwich]);

        await Assert.ThrowsAsync<OrderException>(() =>
            _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
                [new CreateOrderItemDto(sandwich.Id, 2, "")])));
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
                new CreateOrderItemDto(sandwich1.Id, 1, ""),
                new CreateOrderItemDto(sandwich2.Id, 1, "")
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
                new CreateOrderItemDto(drink1.Id, 1, ""),
                new CreateOrderItemDto(drink2.Id, 1, "")
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
            new CreateOrderItemDto(sandwich.Id, 1, ""),
            new CreateOrderItemDto(drink.Id,    1, ""),
            new CreateOrderItemDto(fries.Id,    1, "")
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
            new CreateOrderItemDto(sandwich.Id, 1, ""),
            new CreateOrderItemDto(drink.Id,    1, "")
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
            new CreateOrderItemDto(sandwich.Id, 1, ""),
            new CreateOrderItemDto(fries.Id,    1, "")
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
            [new CreateOrderItemDto(sandwich.Id, 1, "")]));

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
            [new CreateOrderItemDto(fries.Id, 1, "")]));

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
            [new CreateOrderItemDto(sandwich.Id, 1, "")]));

        Assert.Null(result.CustomerId);
        Assert.Equal("João", result.CustomerName);
        Assert.Equal("11999999999", result.CustomerPhone);
        Assert.Equal("Rua A, 1", result.CustomerAddress);
    }

    [Fact]
    public async Task CreateAsync_AnonymousCustomer_SetsOrderDateToUtcNow()
    {
        var before = Datetime.Now;
        var sandwich = BuildProductEntity(type: ProductType.Sandwich, price: 5.00m);
        SetupBasicMocks(products: [sandwich]);

        var result = await _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
            [new CreateOrderItemDto(sandwich.Id, 1, "")]));

        Assert.True(result.OrderDate >= before);
    }

    [Fact]
    public async Task CreateAsync_AnonymousCustomer_ItemObservationTrimmed()
    {
        var sandwich = BuildProductEntity(type: ProductType.Sandwich, price: 5.00m);
        SetupBasicMocks(products: [sandwich]);

        var result = await _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
            [new CreateOrderItemDto(sandwich.Id, 1, " sem cebola ")]));

        Assert.Equal("sem cebola", result.Items.Single().Observation);
    }

    // ── Cardápio real: combos completos ───────────────────────────────────────
    // Preços: X Burger R$5,00 | X Egg R$4,50 | X Bacon R$7,00
    //         Batata Frita R$2,00 | Refrigerante R$2,50

    [Fact]
    public async Task CreateAsync_XBurgerComboCompleto_20PorCentoDesconto()
    {
        // 5,00 + 2,00 + 2,50 = 9,50 → desconto 1,90 → total 7,60
        var sandwich = BuildProductEntity(ProductType.Sandwich, 5.00m, "X Burger");
        var fries    = BuildProductEntity(ProductType.Fries,    2.00m, "Batata Frita");
        var drink    = BuildProductEntity(ProductType.Drink,    2.50m, "Refrigerante");
        SetupBasicMocks(products: [sandwich, fries, drink]);

        var result = await _sut.CreateAsync(AnonymousDto(
            (sandwich.Id, 1, 5.00m), (fries.Id, 1, 2.00m), (drink.Id, 1, 2.50m)));

        Assert.Equal(9.50m, result.Subtotal);
        Assert.Equal(1.90m, result.Discount);
        Assert.Equal(7.60m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_XEggComboCompleto_20PorCentoDesconto()
    {
        // 4,50 + 2,00 + 2,50 = 9,00 → desconto 1,80 → total 7,20
        var sandwich = BuildProductEntity(ProductType.Sandwich, 4.50m, "X Egg");
        var fries    = BuildProductEntity(ProductType.Fries,    2.00m, "Batata Frita");
        var drink    = BuildProductEntity(ProductType.Drink,    2.50m, "Refrigerante");
        SetupBasicMocks(products: [sandwich, fries, drink]);

        var result = await _sut.CreateAsync(AnonymousDto(
            (sandwich.Id, 1, 4.50m), (fries.Id, 1, 2.00m), (drink.Id, 1, 2.50m)));

        Assert.Equal(9.00m, result.Subtotal);
        Assert.Equal(1.80m, result.Discount);
        Assert.Equal(7.20m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_XBaconComboCompleto_20PorCentoDesconto()
    {
        // 7,00 + 2,00 + 2,50 = 11,50 → desconto 2,30 → total 9,20
        var sandwich = BuildProductEntity(ProductType.Sandwich, 7.00m, "X Bacon");
        var fries    = BuildProductEntity(ProductType.Fries,    2.00m, "Batata Frita");
        var drink    = BuildProductEntity(ProductType.Drink,    2.50m, "Refrigerante");
        SetupBasicMocks(products: [sandwich, fries, drink]);

        var result = await _sut.CreateAsync(AnonymousDto(
            (sandwich.Id, 1, 7.00m), (fries.Id, 1, 2.00m), (drink.Id, 1, 2.50m)));

        Assert.Equal(11.50m, result.Subtotal);
        Assert.Equal(2.30m, result.Discount);
        Assert.Equal(9.20m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_XBaconMaisRefri_15PorCentoDesconto()
    {
        // 7,00 + 2,50 = 9,50 → desconto 1,425 → total 8,075
        var sandwich = BuildProductEntity(ProductType.Sandwich, 7.00m, "X Bacon");
        var drink    = BuildProductEntity(ProductType.Drink,    2.50m, "Refrigerante");
        SetupBasicMocks(products: [sandwich, drink]);

        var result = await _sut.CreateAsync(AnonymousDto(
            (sandwich.Id, 1, 7.00m), (drink.Id, 1, 2.50m)));

        Assert.Equal(9.50m, result.Subtotal);
        Assert.Equal(1.425m, result.Discount);
        Assert.Equal(8.075m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_XEggMaisBatata_10PorCentoDesconto()
    {
        // 4,50 + 2,00 = 6,50 → desconto 0,65 → total 5,85
        var sandwich = BuildProductEntity(ProductType.Sandwich, 4.50m, "X Egg");
        var fries    = BuildProductEntity(ProductType.Fries,    2.00m, "Batata Frita");
        SetupBasicMocks(products: [sandwich, fries]);

        var result = await _sut.CreateAsync(AnonymousDto(
            (sandwich.Id, 1, 4.50m), (fries.Id, 1, 2.00m)));

        Assert.Equal(6.50m, result.Subtotal);
        Assert.Equal(0.65m, result.Discount);
        Assert.Equal(5.85m, result.Total);
    }

    // ── Cardápio real: sem desconto ───────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_BebidaOnly_SemDesconto()
    {
        var drink = BuildProductEntity(ProductType.Drink, 2.50m, "Refrigerante");
        SetupBasicMocks(products: [drink]);

        var result = await _sut.CreateAsync(AnonymousDto((drink.Id, 1, 2.50m)));

        Assert.Equal(2.50m, result.Subtotal);
        Assert.Equal(0m, result.Discount);
        Assert.Equal(2.50m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_BebidaMaisBatata_SemSanduiche_SemDesconto()
    {
        // Desconto só existe quando há sanduíche no pedido
        var drink = BuildProductEntity(ProductType.Drink, 2.50m, "Refrigerante");
        var fries = BuildProductEntity(ProductType.Fries, 2.00m, "Batata Frita");
        SetupBasicMocks(products: [drink, fries]);

        var result = await _sut.CreateAsync(AnonymousDto(
            (drink.Id, 1, 2.50m), (fries.Id, 1, 2.00m)));

        Assert.Equal(4.50m, result.Subtotal);
        Assert.Equal(0m, result.Discount);
        Assert.Equal(4.50m, result.Total);
    }

    // ── Validações adicionais de duplicatas ───────────────────────────────────

    [Fact]
    public async Task CreateAsync_DuplicateFries_ThrowsOrderException()
    {
        var fries1 = BuildProductEntity(ProductType.Fries, 2.00m, "Batata Frita");
        var fries2 = BuildProductEntity(ProductType.Fries, 2.00m, "Batata Frita");
        SetupBasicMocks(products: [fries1, fries2]);

        await Assert.ThrowsAsync<OrderException>(() =>
            _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
            [
                new CreateOrderItemDto(fries1.Id, 1, ""),
                new CreateOrderItemDto(fries2.Id, 1, "")
            ])));
    }

    [Fact]
    public async Task CreateAsync_QuantidadeMaiorQueUm_Batata_ThrowsOrderException()
    {
        var fries = BuildProductEntity(ProductType.Fries, 2.00m, "Batata Frita");
        SetupBasicMocks(products: [fries]);

        await Assert.ThrowsAsync<OrderException>(() =>
            _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
                [new CreateOrderItemDto(fries.Id, 2, "")])));
    }

    [Fact]
    public async Task CreateAsync_QuantidadeMaiorQueUm_Bebida_ThrowsOrderException()
    {
        var drink = BuildProductEntity(ProductType.Drink, 2.50m, "Refrigerante");
        SetupBasicMocks(products: [drink]);

        await Assert.ThrowsAsync<OrderException>(() =>
            _sut.CreateAsync(new CreateOrderDto(null, "João", "11999999999", "Rua A",
                [new CreateOrderItemDto(drink.Id, 3, "")])));
    }

    // ── Conteúdo dos itens retornados ─────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ComboCompleto_RetornaItensComProdutosCorretos()
    {
        var sandwich = BuildProductEntity(ProductType.Sandwich, 5.00m, "X Burger");
        var fries    = BuildProductEntity(ProductType.Fries,    2.00m, "Batata Frita");
        var drink    = BuildProductEntity(ProductType.Drink,    2.50m, "Refrigerante");
        SetupBasicMocks(products: [sandwich, fries, drink]);

        var result = await _sut.CreateAsync(AnonymousDto(
            (sandwich.Id, 1, 5.00m), (fries.Id, 1, 2.00m), (drink.Id, 1, 2.50m)));

        Assert.Equal(3, result.Items.Count);
        Assert.Contains(result.Items, i => i.Product.Type == ProductType.Sandwich && i.Product.Name == "X Burger");
        Assert.Contains(result.Items, i => i.Product.Type == ProductType.Fries    && i.Product.Name == "Batata Frita");
        Assert.Contains(result.Items, i => i.Product.Type == ProductType.Drink    && i.Product.Name == "Refrigerante");
    }

    [Fact]
    public async Task CreateAsync_PedidoCriado_StatusInicial_EPendente()
    {
        // O status inicial de todo pedido deve ser Pendente
        var sandwich = BuildProductEntity(ProductType.Sandwich, 5.00m);
        OrderEntity? capturedEntity = null;
        _productRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ProductEntity, bool>>>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync([sandwich]);
        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
                      .Callback<OrderEntity, CancellationToken>((e, _) => capturedEntity = e)
                      .ReturnsAsync((OrderEntity e, CancellationToken _) => e);
        _orderRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.CreateAsync(AnonymousDto((sandwich.Id, 1, 5.00m)));

        Assert.NotNull(capturedEntity);
        Assert.Equal(OrderStatus.Pending, capturedEntity!.Status);
    }

    // ── Cliente cadastrado recebe desconto ────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ClienteCadastrado_ComboCompleto_Applies20Percent()
    {
        var customerId = Guid.NewGuid();
        var customer   = BuildCustomerEntity(customerId);
        var sandwich   = BuildProductEntity(ProductType.Sandwich, 5.00m, "X Burger");
        var fries      = BuildProductEntity(ProductType.Fries,    2.00m, "Batata Frita");
        var drink      = BuildProductEntity(ProductType.Drink,    2.50m, "Refrigerante");
        SetupBasicMocks(customerEntity: customer, products: [sandwich, fries, drink]);

        var result = await _sut.CreateAsync(new CreateOrderDto(
            CustomerId: customerId, CustomerName: null!, CustomerPhone: null!, CustomerAddress: null!,
            items:
            [
                new CreateOrderItemDto(sandwich.Id, 1, ""),
                new CreateOrderItemDto(fries.Id,    1, ""),
                new CreateOrderItemDto(drink.Id,    1, "")
            ]));

        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(9.50m, result.Subtotal);
        Assert.Equal(1.90m, result.Discount);
        Assert.Equal(7.60m, result.Total);
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
        OrderDate = Datetime.Now,
        Subtotal = 5.00m,
        Discount = 0m,
        Total = 5.00m,
        Status = OrderStatus.Pending,
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

    private static ProductEntity BuildProductEntity(ProductType type, decimal price, string? name = null)
    {
        var id = Guid.NewGuid();
        return new ProductEntity
        {
            Id = id,
            Name = name ?? type.ToString(),
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
                    StartDate = Datetime.Now.AddDays(-1),
                    EndDate = null,
                    Reason = "Inicial"
                }
            ]
        };
    }

    private static CreateOrderDto AnonymousDto(params (Guid ProductId, int Qty, decimal Price)[] items) =>
        new(null, "João", "11999999999", "Rua A",
            items.Select(i => new CreateOrderItemDto(i.ProductId, i.Qty, "")).ToList());
}
