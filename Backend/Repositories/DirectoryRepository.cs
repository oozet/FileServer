using Microsoft.EntityFrameworkCore;

public interface IDirectoryRepository
{
    Task AddAsync(DirectoryEntity entity);
    Task<DirectoryEntity> GetByNameAndParentAsync(
        string name,
        int? parentDirectoryId,
        string userId
    );
    Task<DirectoryEntity?> GetByIdAsync(int id);
    Task<List<DirectoryEntity>> GetAllByUserId(string userId);
}

public class DirectoryRepository : Repository<DirectoryEntity>, IDirectoryRepository
{
    public DirectoryRepository(AppDbContext context) : base(context) { }


    public async Task AddAsync(DirectoryEntity entity)
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


    public async Task<DirectoryEntity?> GetByIdAsync(int id)
    {
        return await _context.Directories.FirstOrDefaultAsync(dir => dir.Id == id);
    }

    public async Task<List<DirectoryEntity>> GetAllByUserId(string userId)
    {
        return await _context.Directories.Where(dir => dir.UserId == userId).ToListAsync();
    }
}
