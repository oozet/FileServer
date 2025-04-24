using Microsoft.EntityFrameworkCore;

public interface IDirectoryRepository
{
    Task AddSync(DirectoryEntity entity);
    Task<DirectoryEntity> GetByNameAndParentAsync(
        string name,
        int? parentDirectoryId,
        string userId
    );
}

public class DirectoryRepository : IDirectoryRepository
{
    private readonly AppDbContext _context;

    public DirectoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddSync(DirectoryEntity entity)
    {
        await _context.Directories.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public Task<DirectoryEntity> GetByNameAndParentAsync(
        string name,
        int? parentDirectoryId,
        string userId
    )
    {
        throw new NotImplementedException();
    }
}
