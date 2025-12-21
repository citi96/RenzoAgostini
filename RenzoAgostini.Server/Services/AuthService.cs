using System.Net;
using Microsoft.AspNetCore.Identity;
using RenzoAgostini.Server.Entities;
using RenzoAgostini.Server.Exceptions;
using RenzoAgostini.Server.Services.Interfaces;

namespace RenzoAgostini.Server.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager, ILogger<AuthService> logger) : IAuthService
    {
        public async Task<ApplicationUser> ValidateUserCredentialsAsync(string token)
        {
            
            var user = ApplicationUser.FromGoogleJwt(token);
            if (user == null)
            {
                logger.LogWarning($"Validation failed for user -  Invalid username or password.");
                throw new ApiException(HttpStatusCode.Unauthorized, "Invalid username or password.");
            }

            var dbUser = await userManager.FindByIdAsync(user.Id);
            if (dbUser == null)
                await CreateUserAsync(user);

            logger.LogInformation($"Credentials validated successfully for username {user.Id}");
            return user;
        }

        public async Task<ApplicationUser> GetApplicationUserAsync(string token)
        {
            var user = ApplicationUser.FromGoogleJwt(token);
            if (user == null)
            {
                logger.LogError($"Validation failed");
                throw new ApiException(HttpStatusCode.Unauthorized, "Invalid token");
            }

            var dbUser = await userManager.FindByIdAsync(user.Id);
            if (dbUser == null)
            {
                logger.LogError($"User not signed in {user.Id}");
                throw new ApiException(HttpStatusCode.Forbidden, "User not signed in");
            }

            logger.LogInformation($"Credentials validated successfully for username {user.Id}");
            return dbUser;
        }

        public async Task CreateUserAsync(ApplicationUser user)
        {
            logger.LogInformation($"Creating external user {user.Id}");

            var creationResult = await userManager.CreateAsync(user);
            if (!creationResult.Succeeded)
            {
                var errors = string.Join(", ", creationResult.Errors.Select(e => e.Description));
                logger.LogError($"Failed to create external user: {errors}");
                throw new ApiException(HttpStatusCode.InternalServerError, $"Error creating user: {errors}");
            }

            logger.LogInformation($"External user created successfully with ID {user.Id}");
        }
    }
}