using Microsoft.EntityFrameworkCore;

public interface ITokenRepository
{
    Task AddTokenAsync(TokenInfo token);
    Task UpdateTokenAsync(TokenInfo tokenInfo);
    Task<TokenInfo?> GetTokenByUsernameAsync(string username);
    Task<TokenInfo?> GetTokenInfoAsync(string refreshToken);
    Task DeleteTokenAsync(string username);
}

public class TokenRepository : ITokenRepository
{
    private readonly AppDbContext _context;

    public TokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddTokenAsync(TokenInfo token)
    {
        await _context.TokenStore.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTokenAsync(TokenInfo tokenInfo)
    {
        // Ensure the entity is tracked if it's detached
        if (_context.Entry(tokenInfo).State == EntityState.Detached)
        {
            _context.Attach(tokenInfo);
            _context.Entry(tokenInfo).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync();
    }


    public async Task<TokenInfo?> GetTokenByUsernameAsync(string username)
    {
        return await _context.TokenStore.FirstOrDefaultAsync(t => t.UserName == username);
    }

    public async Task<TokenInfo?> GetTokenInfoAsync(string refreshToken)
    {
        return await _context.TokenStore.FirstOrDefaultAsync(t => t.RefreshToken == refreshToken);
    }

    public async Task DeleteTokenAsync(string username)
    {
        var token = await GetTokenByUsernameAsync(username);
        if (token != null)
        {
            _context.TokenStore.Remove(token);
            await _context.SaveChangesAsync();
        }
    }
}
