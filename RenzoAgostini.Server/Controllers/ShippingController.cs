using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenzoAgostini.Server.Services.Interfaces;
using RenzoAgostini.Shared.Constants;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingController(IShippingOptionService shippingService, ILogger<ShippingController> logger)
        : ControllerBase
    {
        [HttpGet("options")]
        [AllowAnonymous]
        public async Task<ActionResult<IReadOnlyList<ShippingOptionDto>>> GetOptions([FromQuery] string? country = null)
        {
            var resolvedCountry = string.IsNullOrWhiteSpace(country) ? "Italy" : country;
            var options = await shippingService.GetActiveForCountryAsync(resolvedCountry);
            logger.LogInformation("Returned {Count} shipping options for {Country}", options.Count, resolvedCountry);
            return Ok(options);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<IReadOnlyList<ShippingOptionDto>>> GetAll()
        {
            var options = await shippingService.GetAllAsync();
            return Ok(options);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<ShippingOptionDto>> Create([FromBody] CreateShippingOptionDto dto)
        {
            var created = await shippingService.CreateAsync(dto);
            logger.LogInformation("Admin created shipping option {Name} ({Id})", created.Name, created.Id);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<ShippingOptionDto>> Update(int id, [FromBody] UpdateShippingOptionDto dto)
        {
            var updated = await shippingService.UpdateAsync(id, dto);
            logger.LogInformation("Admin updated shipping option {Name} ({Id})", updated.Name, updated.Id);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            await shippingService.DeleteAsync(id);
            logger.LogInformation("Admin deleted shipping option {Id}", id);
            return NoContent();
        }
    }
}
