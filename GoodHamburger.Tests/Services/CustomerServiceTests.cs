using GoodHamburger.API.Entities.Customers;
using GoodHamburger.API.Entities.PurchaseOrders;
using GoodHamburger.API.Repositories.Customers;
using GoodHamburger.API.Services.Customers;
using GoodHamburger.Shared.DTOs.Customers;
using Moq;
using System.Linq.Expressions;

namespace GoodHamburger.Tests.Services;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repoMock = new();
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _sut = new CustomerService(_repoMock.Object);
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CustomerService(null!));
    }

    // ── GetAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_ExistingCustomer_ReturnsCustomer()
    {
        var id = Guid.NewGuid();
        var entity = BuildCustomer(id);
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var result = await _sut.GetAsync(new GetCustomerDto(id));

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Bruno", result.Name);
        Assert.Equal("11999999999", result.Phone);
        Assert.Equal("Rua A, 1", result.Address);
    }

    [Fact]
    public async Task GetAsync_NonExistingCustomer_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CustomerEntity?)null);

        var result = await _sut.GetAsync(new GetCustomerDto(Guid.NewGuid()));

        Assert.Null(result);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllCustomersMapped()
    {
        var entities = new List<CustomerEntity> { BuildCustomer(), BuildCustomer() };
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(entities);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    // ── FindAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task FindAsync_MatchingCustomers_ReturnsMappedResults()
    {
        var entities = new List<CustomerEntity> { BuildCustomer() };
        _repoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CustomerEntity, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entities);

        var result = await _sut.FindAsync(new FindCustomerDto(Name: "Bruno", Phone: null));

        Assert.Single(result);
    }

    [Fact]
    public async Task FindAsync_NoMatch_ReturnsEmpty()
    {
        _repoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CustomerEntity, bool>>>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync([]);

        var result = await _sut.FindAsync(new FindCustomerDto(Name: "Inexistente", Phone: null));

        Assert.Empty(result);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_TrimsWhitespace_AndPersists()
    {
        var entity = BuildCustomer();
        _repoMock.Setup(r => r.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.CreateAsync(new CreateCustomerDto(" Bruno ", " 11999999999 ", " Rua A, 1 "));

        Assert.NotNull(result);
        _repoMock.Verify(r => r.AddAsync(It.Is<CustomerEntity>(e =>
            e.Name == "Bruno" && e.Phone == "11999999999" && e.Address == "Rua A, 1"),
            It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingCustomer_UpdatesAndReturns()
    {
        var id = Guid.NewGuid();
        var entity = BuildCustomer(id, name: "Updated");
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var result = await _sut.UpdateAsync(new UpdateCustomerDto(id, " Updated ", "11988888888", "Rua B, 2"));

        Assert.Equal(id, result.Id);
        _repoMock.Verify(r => r.UpdateAsync(It.Is<CustomerEntity>(e => e.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_GetAfterUpdateReturnsNull_ThrowsInvalidOperationException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((CustomerEntity?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.UpdateAsync(new UpdateCustomerDto(id, "X", "11900000000", "Rua C")));
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_CustomerNotFound_ThrowsInvalidOperationException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CustomerEntity?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.DeleteAsync(new UpdateCustomerDto(Guid.NewGuid(), "X", "X", "X")));
    }

    [Fact]
    public async Task DeleteAsync_CustomerWithOrders_ThrowsInvalidOperationException()
    {
        var entity = BuildCustomer(orders: [new OrderEntity()]);
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.DeleteAsync(new UpdateCustomerDto(entity.Id, "X", "X", "X")));
    }

    [Fact]
    public async Task DeleteAsync_CustomerWithNoOrders_DeletesSuccessfully()
    {
        var entity = BuildCustomer(orders: []);
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repoMock.Setup(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        await _sut.DeleteAsync(new UpdateCustomerDto(entity.Id, "X", "X", "X"));

        _repoMock.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static CustomerEntity BuildCustomer(Guid? id = null, string name = "Bruno",
        ICollection<OrderEntity>? orders = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = name,
        Phone = "11999999999",
        Address = "Rua A, 1",
        Orders = orders ?? []
    };
}
