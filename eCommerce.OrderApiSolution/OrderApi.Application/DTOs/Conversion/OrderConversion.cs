using OrderApi.Domain.Entities;
namespace OrderApi.Application.DTOs.Conversion
{
    public static class OrderConversion
    {
        public static Order ToEntity(OrderDTO order) => new Order()
        {
            Id = order.Id,
            ClientId = order.ClientId,
            ProductId = order.ProductId,
            OrderedDate = order.OrderedDate,
            PurchaseQuantity = order.PurchaseQuantity
        };

        public static (OrderDTO, IEnumerable<OrderDTO>?) FromEntity(Order? order, IEnumerable<Order>? orders)
        {
            //return single
            if (order is not null || orders is null)
            {
                var singleOrder = new OrderDTO(
                    order!.Id,
                    order.ProductId,
                    order!.ClientId,
                    order.PurchaseQuantity,
                    order.OrderedDate);
                return (singleOrder, null);
            }

            //return list
            if (order is null || orders is not null)
            {
                var _orders = orders!.Select(o =>
                    new OrderDTO(
                        o.Id,
                        o!.ClientId,
                        o.ProductId,
                        o.PurchaseQuantity,
                        o.OrderedDate));
                return (null, _orders)!;
            }

            return (null, null)!;
        }
    }
}
