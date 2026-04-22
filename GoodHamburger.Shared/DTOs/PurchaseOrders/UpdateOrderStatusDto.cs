using GoodHamburger.Shared.Enums;

namespace GoodHamburger.Shared.DTOs.PurchaseOrders;

public sealed record UpdateOrderStatusDto(Guid Id, OrderStatus Status);
