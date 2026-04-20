using GoodHamburger.Shared.Models.Products;

namespace GoodHamburger.Shared.Models.PurchaseOrders;

public record OrderItem(int Quantity,
                        decimal UnitPrice,
                        string Observation,
                        Product Product);

