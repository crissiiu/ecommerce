using OrderApi.Application.DTOs;
using OrderApi.Application.DTOs.Conversion;
using OrderApi.Application.Interfaces;
using Polly.Registry;
using System.Net.Http.Json;

namespace OrderApi.Application.Services
{
    public class OrderService(IOrder orderInterface, HttpClient httpClient,
        ResiliencePipelineProvider<string> resiliencePipeline) : IOrderService
    {
        //Get Product
        public async Task<ProductDTO> GetProductAsync(int productId)
        {
            //Call Product Api Using HttpClient
            //Redirect this call the api gateway since product api is not response to outsiders.
            var getProduct = await httpClient.GetAsync($"api/products/{productId}");
            if (!getProduct.IsSuccessStatusCode)
            {
                return null!;
            }
            var product = await getProduct.Content.ReadFromJsonAsync<ProductDTO>();
            return product!;
        }

        //Get User 
        public async Task<AppUserDTO> GetUserAsync(int userId)
        {
            //Call Product Api Using HttpClient
            //Redirect this call the api gateway since product api is not response to outsiders.
            var getUser = await httpClient.GetAsync($"api/authentication/{userId}");
            if (!getUser.IsSuccessStatusCode)
            {
                return null!;
            }
            var user = await getUser.Content.ReadFromJsonAsync<AppUserDTO>();
            return user!;
        }

        //Get Order Detail by id
        public async Task<OrderDetailsDTO> GetOrderDetails(int orderId)
        {
            //Prepare Order 
            var order = await orderInterface.GetByIdAsync(orderId);
            if (order is null || order!.Id < 0)
            {
                return null!;
            }

            //Get Retry pipeline
            var retryPipeline = resiliencePipeline.GetPipeline("my-retry-pipeline");

            //Prepare Product
            var productDTO = await retryPipeline.ExecuteAsync(async token => await GetProductAsync(order.ProductId));

            //Prepare Client
            var appUserDTO = await retryPipeline.ExecuteAsync(async token => await GetUserAsync(order.ClientId));

            //Pupulate order details
            return new OrderDetailsDTO(
                order.Id,
                productDTO.Id,
                appUserDTO.Id,
                appUserDTO.Name,
                appUserDTO.Email,
                appUserDTO.Address,
                appUserDTO.TelephoneNumber,
                productDTO.Name,
                order.PurchaseQuantity,
                productDTO.Price,
                productDTO.Price * order.PurchaseQuantity,
                order.OrderedDate
                );
        }

        //Get Order By Client Id
        public async Task<IEnumerable<OrderDTO>> GetOrdersByClientId(int clientId)
        {
            //Get all client orders
            var orders = await orderInterface.GetOrdersAsync(o => o.ClientId == clientId);
            if (!orders.Any())
            {
                return null!;
            }

            //Conver from entity to DTO
            var (_, _orders) = OrderConversion.FromEntity(null, orders);
            return _orders!;
        }
    }
}
