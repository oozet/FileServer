using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public interface IUserService
{
    Task<bool> CreateUserAsync(string username, string email, string password);
    Task<AppUser> ValidateUserAsync(LoginDto model);
    Task<List<Claim>> GenerateClaimsAsync(AppUser user);
    Task<AppUser> GetUserAsync(string userIdentifier);
    Task<UserDto> GetUserByIdAsync(string userId);
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordDto model);
}

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public UserService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService,
        ILogger<AuthController> logger
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<bool> CreateUserAsync(string username, string email, string password)
    {
        try
        {
            var user = new AppUser { UserName = username, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new Exception("Unable to create user");
            }

            await _userManager.AddToRoleAsync(user, AppRoles.User);
            return true;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error while creating user: {Username}", username);

            throw;
        }
    }

    public async Task<AppUser> ValidateUserAsync(LoginDto model)
    {
        try
        {
            // Try to login in user
            var user =
                await _userManager.FindByNameAsync(model.UserName)
                ?? await _userManager.FindByEmailAsync(model.UserName);
            if (user == null)
            {
                throw new NullReferenceException();
            }
            bool isValidPassword = await _userManager.CheckPasswordAsync(user, model.Password);
            if (isValidPassword == false)
            {
                throw new UnauthorizedAccessException();
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<UserDto> GetUserByIdAsync(string userId)
    {
        try
        {
            // Retrieve user information.
            var user =
                await _userManager.FindByIdAsync(userId) ?? throw new NullReferenceException();

            return new UserDto
            {
                UserId = userId,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<AppUser> GetUserAsync(string userIdentifier)
    {
        try
        {
            // Retrieve user from database.
            var user =
                (
                    await _userManager.FindByNameAsync(userIdentifier)
                    ?? await _userManager.FindByEmailAsync(userIdentifier)
                ) ?? throw new NullReferenceException();

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        var user =
            await _userManager.FindByEmailAsync(email)
            ?? throw new NullReferenceException("No user connected to that email.");

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            throw new NullReferenceException("User does not exist.");
        }

        // Reset the password using the provided token
        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

        if (!result.Succeeded)
        {
            throw new Exception("Unable to reset password");
        }

        return true;
    }

    public async Task<List<Claim>> GenerateClaimsAsync(AppUser user)
    {
        // Creating the necessary claims
        List<Claim> authClaims =
        [
            new(
                ClaimTypes.Name,
                user.UserName
                    ?? throw new InvalidOperationException("Critical error: UserName is null")
            ),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // unique id for token
        ];

        var userRoles = await _userManager.GetRolesAsync(user);

        // Adding roles to the claims. So that we can get the user role from the token.
        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }
        return authClaims;
    }

    public Task<AppUser> LoginUserAsync(LoginDto model)
    {
        throw new NotImplementedException();
    }
}
