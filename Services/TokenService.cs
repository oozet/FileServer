using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public interface ITokenService
{
    Task<string> SaveTokenAsync(string username);
    Task<bool> RevokeAsync(string username);
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration configuration, AppDbContext context, ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }

    public async Task<string> SaveTokenAsync(string username)
    {

        try
        {
            string refreshToken = GenerateRefreshToken();
            var tokenInfo = await _context.TokenInfo.FirstOrDefaultAsync(a => a.UserName == username);
    
            // If tokenInfo is null for the user, create a new one
            if (tokenInfo == null)
            {
                var ti = new Token
                {
                    UserName = username,
                    RefreshToken = refreshToken,
                    ExpiredAt = DateTime.UtcNow.AddDays(7)
                };
                await _context.TokenInfo.AddAsync(ti);
            }
            // Else, update the refresh token and expiration
            else
            {
                tokenInfo.RefreshToken = refreshToken;
                tokenInfo.ExpiredAt = DateTime.UtcNow.AddDays(7);
            }
    
            await _context.SaveChangesAsync();
    
            return tokenInfo!.RefreshToken;
        }
        catch (System.Exception ex)
        {
            
            Console.WriteLine(ex.ToString());
            return "Bad request";
        }
    }

    public async Task<bool> RevokeAsync(string username)
{
    try
    {
        var user = _context.TokenInfo.SingleOrDefault(u => u.UserName == username);
        if (user == null)
        {
            throw new ArgumentNullException(nameof(username),"Token with that username missing from database.");
        }
            Console.WriteLine(user.RefreshToken);
        user.RefreshToken = string.Empty;
        await _context.SaveChangesAsync();

        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex.Message);
        return false;
    }
}

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        // Create a symmetric security key using the secret key from the configuration.
        var authSigningKey = new SymmetricSecurityKey
                        (Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret missing in TokenService")));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _configuration["JWT:ValidIssuer"],
            Audience = _configuration["JWT:ValidAudience"],
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(10),
            SigningCredentials = new SigningCredentials
                          (authSigningKey, SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        // Create a 32-byte array to hold cryptographically secure random bytes
        var randomNumber = new byte[32];

        // Use a cryptographically secure random number generator 
        // to fill the byte array with random values
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomNumber);

        // Convert the random bytes to a base64 encoded string 
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken)
    {
        // Define the token validation parameters used to validate the token.
   var tokenValidationParameters = new TokenValidationParameters
   {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = _configuration["JWT:ValidAudience"],
            ValidIssuer = _configuration["JWT:ValidIssuer"],
            ValidateLifetime = false, 
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey
                  (Encoding.UTF8.GetBytes(_configuration["JWT:secret"] ?? throw new InvalidOperationException("Something wrong with getfromexp")))
   };

   var tokenHandler = new JwtSecurityTokenHandler();

   // Validate the token and extract the claims principal and the security token.
   var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);

    // Cast the security token to a JwtSecurityToken for further validation.
   var jwtSecurityToken = securityToken as JwtSecurityToken;

   // Ensure the token is a valid JWT and uses the HmacSha256 signing algorithm.
   // If no throw new SecurityTokenException
   if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals
   (SecurityAlgorithms.HmacSha256,StringComparison.InvariantCultureIgnoreCase))
   {
       throw new SecurityTokenException("Invalid token");
   }

   // return the principal
   return principal;
    }
}