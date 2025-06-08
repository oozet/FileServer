using Microsoft.EntityFrameworkCore;

public interface IDirectoryRepository : IRepository<DirectoryEntity>
{
    Task<DirectoryEntity> GetByNameAndParentAsync(
        string name,
        int? parentDirectoryId,
        string userId
    );
    Task<List<DirectoryEntity>> GetAllByUserId(string userId);
    Task<DirectoryEntity?> GetWithRelationsAsync(int id);
}

public class DirectoryRepository : Repository<DirectoryEntity>, IDirectoryRepository
{
    public DirectoryRepository(AppDbContext context) : base(context) { }

    public Task<DirectoryEntity> GetByNameAndParentAsync(
        string name,
        int? parentDirectoryId,
        string userId
    )
    {
        throw new NotImplementedException();
    }

    public async Task<List<DirectoryEntity>> GetAllByUserId(string userId)
    {
        return await _context.Directories.Where(dir => dir.UserId == userId).ToListAsync();
    }

    public async Task<DirectoryEntity?> GetWithRelationsAsync(int id)
    {
        return await _context.Directories
        .Include(d => d.Files)
        .Include(d => d.ChildDirectories)
        .FirstOrDefaultAsync(d => d.Id == id);
    }
}
