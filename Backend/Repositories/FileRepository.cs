using Microsoft.EntityFrameworkCore;

public interface IFileRepository
{
    public Task AddAsync(FileEntity fileEntity);
    public Task<FileEntity?> GetAsync(string fileId);
    public Task<List<FileEntity>> GetFilesByUserIdAsync(string userId);
}

public class FileRepository : Repository<FileEntity>, IFileRepository
{
    public FileRepository(AppDbContext context)
        : base(context) { }

    public new async Task AddAsync(FileEntity fileEntity)
    {
        var directory =
            await _context.Directories.FindAsync(fileEntity.DirectoryId)
            ?? throw new Exception("Directory missing");
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            directory.Files.Add(fileEntity);
            _context.Directories.Update(directory);

            await _context.Files.AddAsync(fileEntity);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<FileEntity?> GetAsync(string fileId)
    {
        return await _context.Files.FirstOrDefaultAsync(file => file.Id == fileId);
    }

    public async Task<List<FileEntity>> GetFilesByUserIdAsync(string userId)
    {
        return await _context.Files.Where(file => file.UserId == userId).ToListAsync();
    }
}
