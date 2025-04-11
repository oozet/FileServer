using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService,
        SignInManager<AppUser> signInManager,
        TokenService tokenService,
        ILogger<AuthController> logger
    )
    {
        _userService = userService;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string username, string email, string password)
    {
        try
        {
            await _userService.CreateUserAsync(username, email, password);

            return Ok("User created successfully!");
        }
        catch
        {
            return BadRequest("Unable to create user.");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        try
        {
            // Try to login in user
            var user = await _userService.LoginUserAsync(model);

            // Generate Claims from AppUser
            var authClaims = await GenerateClaimsAsync(user);

            // Generating access token
            var accessToken = _tokenService.GenerateAccessToken(authClaims);

            // Save refreshToken with exp date in the database
            var refreshToken = await _tokenService.SaveTokenInfoAsync(user.UserName!);
            // Set refresh token in HTTP-only secure cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // HTTPS required
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7),
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

            return Ok(
                new
                {
                    accessToken = accessToken,
                    user = new UserDto
                    {
                        UserId = user.Id,
                        UserName = user.UserName!,
                        Email = user.Email ?? string.Empty,
                    },
                }
            );
        }
        catch (NullReferenceException)
        {
            return NotFound("User doesn't exist.");
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred: {Message}", ex.Message);
            return BadRequest();
        }
    }

    [Authorize]
    [HttpGet("get-user")]
    public async Task<IActionResult> GetUserAsync()
    {
        try
        {
            // Retrieve user info from the HttpContext (e.g., ClaimsPrincipal)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Assuming 'id' is in the claims
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var userDto = new UserDto
            {
                UserId = userId,
                UserName = user.UserName,
                Email = user.Email ?? string.Empty,
            };

            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    [HttpPost("token/refresh")]
    public async Task<IActionResult> Refresh(string accessToken)
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Refresh token is missing.");
            }

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var username = principal?.Identity?.Name;
            var claims = principal?.Claims;
            if (username == null || claims == null)
                return BadRequest("Invalid refresh token. Please login again.");

            var tokenResult = await _tokenService.ValidateRefreshToken(username, refreshToken);
            if (!tokenResult.Success)
            {
                return BadRequest(tokenResult.ErrorMessage);
            }

            var newAccessToken = _tokenService.GenerateAccessToken(claims);

            // Set refresh token in HTTP-only secure cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // HTTPS required
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7),
            };
            Response.Cookies.Append("refreshToken", tokenResult.Token, cookieOptions);

            return Ok(new { newAccessToken });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Unable to refresh token {accessToken}", accessToken);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [Authorize]
    [HttpGet("secure-data")]
    public IActionResult SecureEndpoint()
    {
        return Ok("This data is only accessible with a valid JWT!");
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        if (User?.Identity?.Name == null)
            throw new InvalidOperationException("Critical error: UserName is null");
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _tokenService.RevokeAsync(User.Identity.Name);
        }
        Response.Cookies.Delete("refreshToken");
        return Ok("Logged out");
    }

    [HttpPost("request-password")]
    public async Task<IActionResult> SendPasswordResetToken(
        [FromBody] ResetPasswordRequestDto model
    )
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return BadRequest("User does not exist.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Send the token to the user (e.g., via email)
        //await _emailService.SendPasswordResetEmail(user.Email, token);

        // ------- WARNING ONLY FOR DEVELOPMENT -----------
        // Return a response with email and token to test functionality because no email service is exists
#if DEBUG
        return Ok(token);
#else
        throw new InvalidOperationException(
            "This code should not be included in production builds!"
        );
#endif

        //return Ok("Password reset token sent.");
    }

    [Authorize]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return BadRequest("User does not exist.");
        }

        // Reset the password using the provided token
        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok("Password successfully reset.");
    }

    private async Task<List<Claim>> GenerateClaimsAsync(AppUser user)
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
}
