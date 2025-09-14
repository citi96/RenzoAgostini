// RenzoAgostini.Server/Controllers/CustomOrdersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomOrdersController(ICustomOrderService customOrderService, ILogger<CustomOrdersController> logger) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<CustomOrderDto>> CreateCustomOrder([FromForm] CreateCustomOrderDto dto)
        {
            var result = await customOrderService.CreateCustomOrderAsync(dto);
            return Ok(result);
        }

        [HttpPost("access")]
        public async Task<ActionResult<CustomOrderDto>> GetByAccessCode([FromBody] AccessCustomOrderDto dto)
        {
            var result = await customOrderService.GetByAccessCodeAsync(dto.AccessCode, dto.CustomerEmail);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<CustomOrderDto>>> GetAllCustomOrders()
        {
            var customOrders = await customOrderService.GetAllCustomOrdersAsync();
            return Ok(customOrders);
        }

        [HttpPost("{id}/accept")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<CustomOrderDto>> AcceptCustomOrder(int id, [FromBody] AcceptCustomOrderDto dto)
        {
            var result = await customOrderService.AcceptCustomOrderAsync(id, dto);
            return Ok(result);
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<CustomOrderDto>> RejectCustomOrder(int id, [FromBody] string? reason)
        {
            var result = await customOrderService.RejectCustomOrderAsync(id, reason);
            return Ok(result);
        }
    }
}