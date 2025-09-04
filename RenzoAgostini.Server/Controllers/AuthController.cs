using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using RenzoAgostini.Server.Services.Interfaces;

namespace RenzoAgostini.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
    {
        [HttpGet("login")]
        public async Task<IActionResult> Login()
        {
            var user = await authService.ValidateUserCredentialsAsync(AuthenticationHeaderValue.Parse(Request.Headers[HeaderNames.Authorization]!).Parameter!);

            logger.LogInformation($"User {user.Id} logged in successfully.");
            return Ok(user);
        }
    }
}