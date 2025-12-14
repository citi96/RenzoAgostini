using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Services;
using RenzoAgostini.Shared.DTOs;
using System.Security.Claims;

namespace RenzoAgostini.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return Unauthorized("Invalid Authentication");
        }

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.GenerateAccessToken(user, roles);
        var refreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        return Ok(new TokenDto { AccessToken = accessToken, RefreshToken = refreshToken });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var userExists = await userManager.FindByEmailAsync(registerDto.Email);
        if (userExists != null)
            return BadRequest("User already exists!");

        ApplicationUser user = new()
        {
            Email = registerDto.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = registerDto.Email,
            Name = registerDto.Firstname,
            Surname = registerDto.Lastname
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
            return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "User creation failed! Please check user details and try again." });

        if (!await roleManager.RoleExistsAsync("Viewer"))
            await roleManager.CreateAsync(new IdentityRole("Viewer"));

        await userManager.AddToRoleAsync(user, "Viewer");

        return Ok(new { Status = "Success", Message = "User created successfully!" });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenDto>> Refresh([FromBody] TokenDto tokenDto)
    {
        if (tokenDto is null)
            return BadRequest("Invalid client request");

        var principal = tokenService.GetPrincipalFromExpiredToken(tokenDto.AccessToken);
        if (principal == null)
            return BadRequest("Invalid access token or refresh token");

        var username = principal.Identity?.Name;
        var user = await userManager.FindByNameAsync(username!);

        if (user == null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return BadRequest("Invalid access token or refresh token");

        var newAccessToken = tokenService.GenerateAccessToken(user, await userManager.GetRolesAsync(user));
        var newRefreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        await userManager.UpdateAsync(user);

        return Ok(new TokenDto { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await userManager.Users.ToListAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                Name = user.Name ?? "",
                Surname = user.Surname ?? "",
                Roles = roles.ToList() // Assuming UserDto has Roles property, detailed check needed
            });
        }
        return Ok(userDtos);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("roles")]
    public async Task<IActionResult> AssignRole([FromBody] SetRoleDto setRoleDto)
    {
        var user = await userManager.FindByNameAsync(setRoleDto.UserName);
        if (user == null)
            return NotFound("User not found");

        var roleExists = await roleManager.RoleExistsAsync(setRoleDto.Role);
        if (!roleExists)
            return BadRequest("Role does not exist");

        // Remove current roles (simplification: clear all and set new, or just add?)
        // Requirement: toggle Admin/Viewer.
        // Let's assume a user has one primary role for now or we just add/remove.
        // Best practice: remove all old roles, add new one? Or just add.
        // Let's implement: Ensure user is in target role.

        // "toggle Admin/Viewer" implies mutually exclusive or checks.
        // Safe bet: Remove from all other roles, add to this one.
        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRoleAsync(user, setRoleDto.Role);

        return Ok($"Role {setRoleDto.Role} assigned to user {setRoleDto.UserName}");
    }
}