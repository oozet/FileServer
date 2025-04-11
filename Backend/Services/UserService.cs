using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public interface IUserService
{
    Task<bool> CreateUserAsync(string username, string email, string password);
    Task<AppUser> LoginUserAsync(LoginDto model);
}

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public UserService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        TokenService tokenService,
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
            _logger.LogError(
                ex,
                "Error while creating user: {Username}",
                username
            );

            throw;
        }
    }

    public async Task<AppUser> LoginUserAsync(LoginDto model)
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
}
