using GoodHamburger.API.Entities.Customers;
using GoodHamburger.API.Entities.Products;
using GoodHamburger.API.Entities.PurchaseOrders;
using GoodHamburger.API.Extensions.Entities;
using GoodHamburger.API.Extensions.Entities.Products;
using GoodHamburger.Shared.Extensions.Models;
using GoodHamburger.Shared.Models.Customers;
using GoodHamburger.Shared.Models.Products;
using GoodHamburger.Shared.Models.PurchaseOrders;

namespace GoodHamburger.API.Extensions.Entities
{
    public static class EntityToModelExtensions
    {
        public static Customer MapEntityToModel(this CustomerEntity entity)
        {
            return new Customer(entity.Id, entity.Name, entity.Phone, entity.Address);
        }

        public static Product MapEntityToModel(this ProductEntity entity, DateTime? dateRef = null)
        {
            return new Product(entity.Id, entity.Name, entity.Description, entity.GetCurrentPrice(dateRef).Value, entity.Type);
        }

        public static ProductPrice MapEntityToModel(this ProductPriceEntity entity)
        {
            return new ProductPrice(entity.Id, entity.ProductId, entity.Value, entity.StartDate, entity.EndDate, entity.Reason);
        }

        public static Order MapEntityToModel(this OrderEntity entity)
        {
            if (entity.CustomerId is null)
                return new Order(entity.Id, entity.OrderDate, entity.Subtotal, entity.Discount, entity.Total, null, entity.CustomerName!, entity.CustomerPhone!, entity.CustomerAddress!, entity.Status, [.. entity.Items.Select(i => i.MapEntityToModel(entity.OrderDate))]);
            else
                return OrderExtension.FromCustomer(entity.Id, entity.OrderDate, entity.Subtotal, entity.Discount, entity.Total, entity.Customer!.MapEntityToModel(), entity.Status, [.. entity.Items.Select(i => i.MapEntityToModel(entity.OrderDate))]);
        }

        public static OrderItem MapEntityToModel(this OrderItemEntity entity, DateTime dateRef)
        {
            return new OrderItem(entity.Quantity, entity.Observation, entity.Product.MapEntityToModel(dateRef));
        }
    }
}
