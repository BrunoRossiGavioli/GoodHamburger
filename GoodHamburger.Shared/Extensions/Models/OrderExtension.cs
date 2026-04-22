using GoodHamburger.Shared.Exceptions.BusinessRules;
using GoodHamburger.Shared.Models.Customers;
using GoodHamburger.Shared.Models.PurchaseOrders;
using System.Numerics;

namespace GoodHamburger.Shared.Extensions.Models
{
    public static class OrderExtension
    {
        public static Order FromCustomer(Guid id, DateTime orderDate, decimal subtotal, decimal discount, decimal total, Customer customer, IReadOnlyCollection<OrderItem> items)
        {
            ArgumentNullException.ThrowIfNull(customer);
            return new(id, orderDate, subtotal, discount, total, customer.Id, customer.Name, customer.Phone, customer.Address, items);
        }

        /// <summary>
        /// Validate the order items based on the business rules
        /// </summary>
        /// <param name="items"></param>
        /// <exception cref="OrderException">Ocorrs when some business rule is not respected</exception>
        public static void ThrowIfInvalidOrder(this IEnumerable<OrderItem> items)
        {
            if (items.Any(i => i.Quantity > 1))
                throw new OrderException("A quantidade máxima por pedido é de uma unidade por produto");

            var productTypes = items.GroupBy(p => p.Product.Type);
            for (var i = 0; i < productTypes.Count(); i++)
            {
                var typeGroup = productTypes.ElementAt(i);
                if(typeGroup.Count() > 1)
                {
                    var typeName = typeGroup.Key.GetDisplayName();
                    throw new OrderException($"Não é possível adicionar mais de um item do tipo {typeName} por pedido");
                }
            }
        }

        /// <summary>
        /// Calculate the subtotal, discount and total of an order based on the items and the discount rules:
        /// </summary>
        /// <returns>
        /// Return a tuple with the subtotal, absolute value of discount amount and total of the order
        /// </returns>
        public static (decimal subtotal, decimal discountAbs, decimal total) CalculateSubtotalAndDiscount(this IEnumerable<OrderItem> items)
        {
            bool hasSandwich = items.Any(i => i.Product.Type == Enums.ProductType.Sandwich);
            bool hasDrink = items.Any(i => i.Product.Type == Enums.ProductType.Drink);
            bool hasFries = items.Any(i => i.Product.Type == Enums.ProductType.Fries);
            decimal discountPercentage = 0;
            if (hasSandwich)
            {
                if (hasDrink && hasFries)
                    discountPercentage = .20m; // 20% off on the complete combo
                else if (hasDrink)
                    discountPercentage = .15m; // 15% off on sandwich + drink
                else if(hasFries)
                    discountPercentage = .10m; // 10% off on sandwich + fries
            }

            decimal subtotal = items.Sum(CalculateItemTotal);
            decimal discount = subtotal * discountPercentage;
            return (subtotal, discount, subtotal - discount);
        }

        private static decimal CalculateItemTotal(OrderItem item)
        {
            return item.Product.Price * item.Quantity;
        }
    }
}
