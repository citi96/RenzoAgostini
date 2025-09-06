using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenzoAgostini.Server.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class OrdersController(IOrderService orderService, ILogger<OrdersController> logger) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            var orderDtos = await orderService.GetAllOrdersAsync();
            return Ok(orderDtos);
        }

        [HttpPut("{id}/tracking")]
        public async Task<ActionResult<OrderDto>> UpdateOrderTracking(int id, [FromBody] UpdateTrackingDto dto)
        {
            var orderDto = await orderService.UpdateOrderTrackingAsync(id, dto.TrackingNumber);
            return Ok(orderDto);
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var orderDto = await orderService.UpdateOrderStatusAsync(id, dto.Status);
            return Ok(orderDto);
        }

        [HttpPost("create-session")]
        [AllowAnonymous]
        public async Task<ActionResult<StripeSessionDto>> CreateStripeCheckoutSession([FromBody] CheckoutDto checkoutDto)
        {
            var result = await orderService.CreateOrderAndStartPaymentAsync(checkoutDto);

            string sessionId = result.Value!;
            string sessionUrl;
            try
            {
                var sessionService = new Stripe.Checkout.SessionService();
                Stripe.Checkout.Session session = sessionService.Get(sessionId);
                sessionUrl = session.Url;
            }
            catch (Stripe.StripeException ex)
            {
                logger.LogError(ex, "Errore nella comunicazione con Stripe.");
                return StatusCode(500, new { Error = "Errore nella comunicazione con Stripe." });
            }

            var sessionDto = new StripeSessionDto(sessionId, sessionUrl);
            return Ok(sessionDto);
        }
    }
}
