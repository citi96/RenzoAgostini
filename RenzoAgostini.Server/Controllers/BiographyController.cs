using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenzoAgostini.Shared.Constants;
using RenzoAgostini.Shared.Contracts;
using RenzoAgostini.Shared.DTOs;

namespace RenzoAgostini.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BiographyController(IBiographyService bioService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<BiographyDto>> Get()
        {
            var result = await bioService.GetAsync();
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult> Update([FromBody] BiographyDto dto)
        {
            await bioService.UpdateAsync(dto);
            return Ok();
        }
    }
}
