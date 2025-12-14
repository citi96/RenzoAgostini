using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using RenzoAgostini.Server.Emailing.Interfaces;
using RenzoAgostini.Server.Emailing.Models;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Services;
using RenzoAgostini.Shared.Constants;
using RenzoAgostini.Shared.DTOs;
using System.Security.Claims;
using System.Text;

namespace RenzoAgostini.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ITokenService tokenService,
    ICustomEmailSender emailSender,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return Unauthorized("Invalid Authentication");
        }

        var roles = (await userManager.GetRolesAsync(user))
            .Select(static role => role.ToLowerInvariant())
            .ToList();
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

        if (!await roleManager.RoleExistsAsync(RoleNames.Viewer))
            await roleManager.CreateAsync(new IdentityRole(RoleNames.Viewer));

        await userManager.AddToRoleAsync(user, RoleNames.Viewer);

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

        var userRoles = (await userManager.GetRolesAsync(user))
            .Select(static role => role.ToLowerInvariant())
            .ToList();

        var newAccessToken = tokenService.GenerateAccessToken(user, userRoles);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        await userManager.UpdateAsync(user);

        return Ok(new TokenDto { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await userManager.Users.ToListAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = (await userManager.GetRolesAsync(user))
                .Select(static role => role.ToLowerInvariant())
                .ToList();
            userDtos.Add(new UserDto
            {
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                Name = user.Name ?? "",
                Surname = user.Surname ?? "",
                Roles = roles // Assuming UserDto has Roles property, detailed check needed
            });
        }
        return Ok(userDtos);
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost("roles")]
    public async Task<IActionResult> AssignRole([FromBody] SetRoleDto setRoleDto)
    {
        var user = await userManager.FindByNameAsync(setRoleDto.UserName);
        if (user == null)
            return NotFound("User not found");

        var normalizedRole = setRoleDto.Role.ToLowerInvariant();

        var roleExists = await roleManager.RoleExistsAsync(normalizedRole);
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
        await userManager.AddToRoleAsync(user, normalizedRole);

        return Ok($"Role {normalizedRole} assigned to user {setRoleDto.UserName}");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return Ok();
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var clientBaseUrl = configuration["ClientBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        var resetUrl = $"{clientBaseUrl}/authentication/reset-password?token={encodedToken}&email={Uri.EscapeDataString(user.Email!)}";

        var emailResult = await emailSender.SendAsync(new EmailMessage(
            From: null,
            ReplyTo: null,
            TextBody: $"Reimposta la tua password usando questo link: {resetUrl}",
            HtmlBody: $"<p>Ciao {user.Name ?? user.Email},</p><p>Per reimpostare la password clicca il pulsante qui sotto.</p><p><a href=\"{resetUrl}\" style=\"padding:12px 18px;border-radius:12px;background:#7c3aed;color:white;text-decoration:none;font-weight:600;\">Reimposta password</a></p>"
        )
        {
            To = [new EmailAddress(user.Email!, user.Name ?? user.Email)]
        });

        if (!emailResult.Success)
        {
            return Problem(emailResult.Error ?? "Impossibile inviare l'email di reset.");
        }

        return Ok();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return BadRequest("Utente non trovato");
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
        var result = await userManager.ResetPasswordAsync(user, decodedToken, dto.Password);

        if (result.Succeeded)
        {
            return Ok();
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return BadRequest(errors);
    }
}