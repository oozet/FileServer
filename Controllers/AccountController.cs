using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly TokenService _tokenService;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, TokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(string username, string email, string password)
    {
        var user = new IdentityUser { UserName = username, Email = email };
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            return Ok("User created successfully!");
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginData)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid login attempt.");

        var user = await _userManager.FindByNameAsync(loginData.UserName) ?? await _userManager.FindByEmailAsync(loginData.UserName);

        if (user == null)
            return Unauthorized("Invalid credentials.");

        var result = await _signInManager.PasswordSignInAsync(user, loginData.Password, isPersistent: false, lockoutOnFailure: false);

        if (!result.Succeeded)
            return Unauthorized("Invalid credentials.");
        var userClaims = await _userManager.GetClaimsAsync(user);
        var token = _tokenService.GenerateAccessToken(userClaims);
        var userToken = new TokenDto { AccessToken = token, RefreshToken = "" };
        // return access token for user's use
        return Ok(userToken);
    }
    
    [Authorize]
    [HttpGet("secure-data")]
    public IActionResult SecureEndpoint()
    {
        return Ok("This data is only accessible with a valid JWT!");
    }

    

    // private JwtSecurityToken GenerateJwtToken(IdentityUser user)
    // {
    //     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key"));

    //     var claims = new List<Claim>
    // {
    //     new Claim(JwtRegisteredClaimNames.Sub, user.Id),
    //     new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
    //     new Claim(ClaimTypes.Name, user.UserName!),
    //     new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    // };

    //     var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    //     return new JwtSecurityToken(
    //         issuer: "yourIssuer",
    //         audience: "yourAudience",
    //         claims: claims,
    //         expires: DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
    //         signingCredentials: credentials
    //     );
    // }
}

// public string GenerateAccessToken(IEnumerable<Claim> claims)
// {
//     var tokenHandler = new JwtSecurityTokenHandler();

//     // Create a symmetric security key using the secret key from the configuration.
//     var authSigningKey = new SymmetricSecurityKey
//                     (Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

//     var tokenDescriptor = new SecurityTokenDescriptor
//     {
//         Issuer = _configuration["JWT:ValidIssuer"],
//         Audience = _configuration["JWT:ValidAudience"],
//         Subject = new ClaimsIdentity(claims),
//         Expires = DateTime.Now.AddMinutes(15),
//         SigningCredentials = new SigningCredentials
//                       (authSigningKey, SecurityAlgorithms.HmacSha256)
//     };

//     var token = tokenHandler.CreateToken(tokenDescriptor);

//     return tokenHandler.WriteToken(token);
// }