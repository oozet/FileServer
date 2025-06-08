using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserService userService,
        ITokenService tokenService,
        ILogger<AuthController> logger
    )
    {
        _userService = userService;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _userService.CreateUserAsync(
                request.Username,
                request.Email,
                request.Password
            );

            if (result.Succeeded)
            {
                return Ok("User created successfully!");
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new ApiError("Registration failed: " + errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while creating user {username}",
                request.Username
            );
            return StatusCode(500, new ApiError("Unable to create user."));
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
                new LoginResponse
                {
                    AccessToken = accessToken,
                    User = new UserDto
                    {
                        Id = appUser.Id,
                        Name = appUser.UserName ?? string.Empty,
                        FirstName = appUser.FirstName,
                        LastName = appUser.LastName,
                        Email = appUser.Email ?? string.Empty,
                    },
                }
            );
        }
        catch (NotFoundException)
        {
            return NotFound(new ApiError("User doesn't exist."));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred: {Message}", ex.Message);
            return StatusCode(500, new ApiError("Server error while trying to log in."));
        }
    }

    [Authorize]
    [HttpGet("get-user-info")]
    public async Task<IActionResult> GetUserInfo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new ApiError("User ID not found."));
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
                        Id = appUser.Id,
                        Name = appUser.UserName ?? string.Empty,
                        Email = appUser.Email ?? string.Empty,
                    },
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Unable to generate access token.");
            return StatusCode(500, new ApiError("Unexpected server error."));
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
                return BadRequest(new ApiError("Refresh token is missing."));
            }

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
            var username = principal?.Identity?.Name;

            var claims = principal?.Claims;
            if (username == null || claims == null)
                return BadRequest(new ApiError("Invalid refresh token. Please login again."));

            var tokenResult = await _tokenService.ValidateRefreshToken(username, refreshToken);
            if (!tokenResult.Success)
            {
                return BadRequest(new ApiError(tokenResult.ErrorMessage));
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
            return StatusCode(500, new ApiError("Unexpected server error."));
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        try
        {
            _logger.LogInformation("Logout attempted.");
            if (User?.Identity?.Name == null)
                throw new InvalidOperationException("Critical error: UserName is null");
            var refreshToken = Request.Cookies["refreshToken"];
            _logger.LogInformation(refreshToken);
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var success = await _tokenService.RevokeAsync(User.Identity.Name);

                _logger.LogInformation("Revoke success?" + success);
            }
            Response.Cookies.Delete("refreshToken");
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiError(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error while trying to log out a user.");
            return StatusCode(500, new ApiError("Unexpected server error."));
        }
    }

    [HttpPost("request-password")]
    public async Task<IActionResult> SendPasswordResetToken([FromBody] string email)
    {
        if (!email.Contains('@') || !email.Contains('.'))
        {
            return BadRequest(new ApiError("Invalid email adress format."));
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
        catch (InvalidOperationException ex)
        {
            _logger.LogCritical(ex, "This endpoint is only implemented for debugging. Needs rework for release mode.");
            return StatusCode(500, new ApiError("Unexpected server error."));
        }
        catch
        {
            _logger.LogError($"Server error while trying to reset password for email: {email}");
            return StatusCode(500, new ApiError("Unexpected server error."));
        }
    }

    [Authorize]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto model)
    {
        try
        {
            await _userService.ResetPasswordAsync(model);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiError(ex.Message));
        }
        catch
        {
            return StatusCode(500, new ApiError("Unable to reset password"));
        }
    }
}
