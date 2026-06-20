using eCommerce.SharedLibrary.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.DTOs;
using OrderApi.Application.DTOs.Conversion;
using OrderApi.Application.Interfaces;
using OrderApi.Application.Services;

namespace OrderApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController(IOrder orderInterface, IOrderService orderService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders()
        {
            var orders = await orderInterface.GetAllAsync();
            if (!orders.Any())
            {
                return NotFound("No order detected in the database");
            }

            var (_, list) = OrderConversion.FromEntity(null, orders);
            return !list!.Any() ? NotFound() : Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDTO>> GetOrder(int id)
        {
            var order = await orderInterface.GetByIdAsync(id);

            if (order is null)
            {
                return NotFound(null);
            }

            var (_order, _) = OrderConversion.FromEntity(order, null);

            return _order is null ? NotFound(null) : Ok(_order);
        }

        [HttpGet("client/{clientId:int}")]
        public async Task<ActionResult<OrderDTO>> GetClientOrder(int clientId)
        {
            if (clientId <= 0)
            {
                return BadRequest("Invalid data provided");
            }
            var orders = await orderInterface.GetOrdersAsync(o => o.ClientId == clientId);

            return orders is null ? NotFound(null) : Ok(orders);
        }

        [HttpGet("details/{orderId:int}")]
        public async Task<ActionResult<OrderDTO>> GetOrderDetails(int orderId)
        {
            if (orderId <= 0)
            {
                return BadRequest("Invalid data provided");
            }
            var orderDetail = await orderService.GetOrderDetails(orderId);

            return orderDetail.OrderId <= 0 ? NotFound("Order not found") : Ok(orderDetail);
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateOrder(OrderDTO orderDTO)
        {
            //check model state if all data annotations are passed
            if (!ModelState.IsValid)
            {
                return BadRequest("Imcomplete data submitted");
            }

            //convert to entity
            var getEntity = OrderConversion.ToEntity(orderDTO);
            var response = await orderInterface.CreateAsync(getEntity);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpPut]
        public async Task<ActionResult<Response>> UpdateOrder(OrderDTO orderDTO)
        {
            //convert to entity
            var getEntity = OrderConversion.ToEntity(orderDTO);
            var response = await orderInterface.UpdateAsync(getEntity);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpDelete]
        public async Task<ActionResult<Response>> DelelteOrder(OrderDTO orderDTO)
        {
            //convert to entity
            var getEntity = OrderConversion.ToEntity(orderDTO);
            var response = await orderInterface.DeleteAsync(getEntity);
            return response.Flag ? Ok(response) : BadRequest(response);
        }
    }
}
