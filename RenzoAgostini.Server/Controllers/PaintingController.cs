using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenzoAgostini.Server.Services.Interfaces;
using RenzoAgostini.Shared.Constants;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaintingsController(IPaintingService paintingService) : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<PaintingDto>> GetAll()
        {
            return await paintingService.GetAllPaintingsAsync();
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PaintingDto>> GetById(int id)
        {
            var painting = await paintingService.GetPaintingByIdAsync(id);
            return painting is null ? NotFound() : Ok(painting);
        }

        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<PaintingDto>> GetBySlug(string slug)
        {
            var painting = await paintingService.GetPaintingBySlugAsync(slug);
            return painting is null ? NotFound() : Ok(painting);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<PaintingDto>> Create(CreatePaintingDto dto)
        {
            var created = await paintingService.CreatePaintingAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult<PaintingDto>> Update(int id, CreatePaintingDto dto)
        {
            var updated = await paintingService.UpdatePaintingAsync(id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            await paintingService.DeletePaintingAsync(id);
            return NoContent();
        }

        [HttpGet("for-sale")]
        public async Task<IEnumerable<PaintingDto>> GetForSale()
        {
            return await paintingService.GetPaintingsForSaleAsync();
        }
    }
}
