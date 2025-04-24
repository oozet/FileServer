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
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService,
        SignInManager<AppUser> signInManager,
        ITokenService tokenService,
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
            var appUser =
                await _userService.ValidateUserAsync(model)
                ?? throw new Exception("Validation failed without throwing error.");

            // Generate Claims from AppUser
            var authClaims = await _userService.GenerateClaimsAsync(appUser);
            foreach (var claim in authClaims)
            {
                Console.WriteLine(claim);
            }

            // Generating access token
            var accessToken = _tokenService.GenerateAccessToken(authClaims);

            // Save refreshToken with exp date in the database
            var refreshToken = await _tokenService.SaveTokenInfoAsync(appUser.UserName!);
            Console.WriteLine("RefreshToken:" + refreshToken);

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
                        UserId = appUser.Id,
                        UserName = appUser.UserName ?? string.Empty,
                        Email = appUser.Email ?? string.Empty,
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
    [HttpGet("get-user-info")]
    public async Task<IActionResult> GetUserInfo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        try
        {
            var userDto = await _userService.GetUserByIdAsync(userId);
            return Ok(userDto);
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpPost("generate-access-token")]
    public async Task<IActionResult> GenerateAccessToken()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Refresh token is missing.");
            }

            var tokenInfo = await _tokenService.GetTokenInfoAsync(refreshToken);
            if (tokenInfo == null)
            {
                return BadRequest("Invalid refresh token.");
            }

            var appUser = await _userService.GetUserAsync(tokenInfo.UserName);
            var claims = await _userService.GenerateClaimsAsync(appUser);

            var newAccessToken = _tokenService.GenerateAccessToken(claims);

            // Set refresh token in HTTP-only secure cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // HTTPS required
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7),
            };
            Response.Cookies.Append("refreshToken", tokenInfo.RefreshToken, cookieOptions);

            return Ok(
                new
                {
                    accessToken = newAccessToken,
                    user = new UserDto
                    {
                        UserId = appUser.Id,
                        UserName = appUser.UserName ?? string.Empty,
                        Email = appUser.Email ?? string.Empty,
                    },
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Unable to generate access token.");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(string accessToken)
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
    public async Task<IActionResult> SendPasswordResetToken([FromBody] string email)
    {
        if (!email.Contains('@') || !email.Contains('.'))
        {
            return BadRequest("Invalid email adress.");
        }
        try
        {
            var token = await _userService.GeneratePasswordResetTokenAsync(email);

            if (token != null)
            {
                // Send the token to the user (e.g., via email)
                //await _emailService.SendPasswordResetEmail(user.Email, token);
#if DEBUG
                return Ok(token);
#else
                throw new InvalidOperationException(
                    "This code should not be included in production builds!"
                );
#endif
            }

            return BadRequest("");
        }
        catch
        {
            return BadRequest("");
        }
    }

    [Authorize]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto model)
    {
        try
        {
            await _userService.ResetPasswordAsync(model);
            return Ok("Password successfully reset.");
        }
        catch (NullReferenceException)
        {
            return NotFound("User not found in database.");
        }
    }
}
