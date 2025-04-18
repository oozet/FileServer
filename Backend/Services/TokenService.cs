using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public interface ITokenService
{
    Task<string> SaveTokenInfoAsync(string username);
    Task<bool> RevokeAsync(string username);
    Task<TokenInfo?> GetTokenInfoAsync(string refreshToken);
    Task<TokenResult> ValidateRefreshToken(string username, string refreshToken);
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ITokenRepository _tokenRepository;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IConfiguration configuration,
        ITokenRepository tokenRepository,
        ILogger<TokenService> logger
    )
    {
        _configuration = configuration;
        _tokenRepository = tokenRepository;
        _logger = logger;
    }

    public async Task<string> SaveTokenInfoAsync(string username)
    {
        try
        {
            string refreshToken = GenerateRefreshToken();

            var tokenInfo = await _tokenRepository.GetTokenByUsernameAsync(username);

            // If tokenInfo is null for the user, create a new one
            if (tokenInfo == null)
            {
                var ti = new TokenInfo
                {
                    UserName = username,
                    RefreshToken = refreshToken,
                    ExpiredAt = DateTime.UtcNow.AddDays(7),
                };
                await _tokenRepository.AddTokenAsync(ti);
            }
            // Else, update the refresh token and expiration
            else
            {
                tokenInfo.RefreshToken = refreshToken;
                tokenInfo.ExpiredAt = DateTime.UtcNow.AddDays(7);

                await _tokenRepository.UpdateTokenAsync(tokenInfo);
            }

            return refreshToken;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while creating refresh token for user: {Username}",
                username
            );
            return "";
        }
    }

    public async Task<bool> RevokeAsync(string username)
    {
        try
        {
            var refreshToken = await _tokenRepository.GetTokenByUsernameAsync(username);
            if (refreshToken == null)
            {
                throw new ArgumentNullException(
                    nameof(username),
                    "Token with that username missing from database."
                );
            }

            await _tokenRepository.DeleteTokenAsync(username);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to revoke refresh token for user: {Username}", username);
            return false;
        }
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        // Create a symmetric security key using the secret key from the configuration.
        var authSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["JWT:Secret"]
                    ?? throw new InvalidOperationException("JWT:Secret missing in TokenService")
            )
        );

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _configuration["JWT:ValidIssuer"],
            Audience = _configuration["JWT:ValidAudience"],
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(10),
            SigningCredentials = new SigningCredentials(
                authSigningKey,
                SecurityAlgorithms.HmacSha256
            ),
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["JWT:secret"]
                        ?? throw new InvalidOperationException("Something wrong with GetPrincipalFromExpiredToken")
                )
            ),
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        // Validate the token and extract the claims principal and the security token.
        var principal = tokenHandler.ValidateToken(
            accessToken,
            tokenValidationParameters,
            out SecurityToken securityToken
        );

        // Ensure the token is a valid JWT and uses the HmacSha256 signing algorithm.
        // If no throw new SecurityTokenException
        if (
            securityToken is not JwtSecurityToken jwtSecurityToken
            || !jwtSecurityToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase
            )
        )
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    public async Task<TokenInfo?> GetTokenInfoAsync(string refreshToken)
    {
        try
        {
            var tokenInfo = await _tokenRepository.GetTokenInfoAsync(refreshToken);
            if (
                tokenInfo == null
                || tokenInfo.RefreshToken != refreshToken
                || tokenInfo.ExpiredAt <= DateTime.UtcNow
            )
            {
                return null;
            }

            var newRefreshToken = GenerateRefreshToken();
            tokenInfo.RefreshToken = newRefreshToken;

            await _tokenRepository.UpdateTokenAsync(tokenInfo);

            return tokenInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unable to validate refresh token {refreshToken}",
                refreshToken
            );
            return null;
        }
    }

    public async Task<TokenResult> ValidateRefreshToken(string username, string refreshToken)
    {
        try
        {
            var tokenInfo = await _tokenRepository.GetTokenByUsernameAsync(username);
            if (
                tokenInfo == null
                || tokenInfo.RefreshToken != refreshToken
                || tokenInfo.ExpiredAt <= DateTime.UtcNow
            )
            {
                return new TokenResult { Success = false, ErrorMessage = "Invalid refreshtoken" };
            }

            var newRefreshToken = GenerateRefreshToken();
            tokenInfo.RefreshToken = newRefreshToken;

            await _tokenRepository.UpdateTokenAsync(tokenInfo);

            return new TokenResult { Success = true, Token = newRefreshToken };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unable to validate refresh token {refreshToken} for user: {Username}",
                refreshToken,
                username
            );
            return new TokenResult
            {
                Success = false,
                ErrorMessage = "Error while validating refreshtoken",
            };
            ;
        }
    }
}
