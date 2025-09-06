using Microsoft.AspNetCore.Mvc;
using RenzoAgostini.Server.Services.Interfaces;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController(IOrderService orderService) : ControllerBase
    {
        [HttpPost("checkout")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutDto checkoutDto)
        {
            var result = await orderService.CreateOrderAndStartPaymentAsync(checkoutDto);
            return Ok(new { SessionId = result.Value });
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody] string sessionId)
        {
            var result = await orderService.ConfirmOrderPaymentAsync(sessionId);
            return Ok(result.Value);
        }


        [HttpGet("cancel")]
        public async Task<IActionResult> PaymentCancelled([FromQuery] string session_id)
        {
            if (!string.IsNullOrEmpty(session_id))
            {
                var order = await orderService.ConfirmOrderPaymentAsync(session_id);
            }

            return Redirect("/checkout-annullato");
        }
    }
}
