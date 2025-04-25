using Microsoft.EntityFrameworkCore;

public interface ITokenRepository
{
    Task AddTokenAsync(TokenInfo entity);
    Task UpdateTokenAsync(TokenInfo tokenInfo);
    Task<TokenInfo?> GetTokenByUsernameAsync(string username);
    Task<TokenInfo?> GetTokenInfoAsync(string refreshToken);
    Task DeleteTokenAsync(string username);
}

public class TokenRepository : Repository<TokenInfo>, ITokenRepository
{
    public TokenRepository(AppDbContext context) : base(context)
    {
    }

    public async Task AddTokenAsync(TokenInfo entity)
    {
        await _context.TokenStore.AddAsync(entity);
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
