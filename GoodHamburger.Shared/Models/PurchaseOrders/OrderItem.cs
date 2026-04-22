using GoodHamburger.Shared.Models.Products;

namespace GoodHamburger.Shared.Models.PurchaseOrders;

public record OrderItem(int Quantity,
                        string Observation,
                        Product Product);

