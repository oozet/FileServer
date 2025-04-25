using Microsoft.EntityFrameworkCore;

public interface IFileRepository : IRepository
{
    public Task AddFileAsync(FileEntity fileEntity);
    public Task<List<FileEntity>> GetFilesByUserIdAsync(string userId);
}

public class FileRepository : Repository<FileEntity>, IFileRepository
{

    public FileRepository(AppDbContext context) : base(context)
    { }

    public async Task AddFileAsync(FileEntity fileEntity)
    {
        var directory = await _context.Directories.FindAsync(fileEntity.DirectoryId) ?? throw new Exception("Directory missing");
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

    // public async Task AddFileDirAsync(FileEntity fileEntity)
    // {
    //     await _context.Files.AddAsync(fileEntity);
    //     await _context.SaveChangesAsync();
    //     using var transaction = await _context.Database.BeginTransactionAsync();
    //     try
    //     {
    //         await _context.
    //         await _context.AddAsync(directoryEntity);
    //         await _context.SaveChangesAsync();

    //         fileEntity.DirectoryId = directoryEntity.Id;
    //         await _context.AddAsync(fileEntity);
    //         await _context.SaveChangesAsync();

    //         await transaction.CommitAsync();
    //     }
    //     catch
    //     {
    //         await transaction.RollbackAsync();
    //         throw;
    //     }
    // }

    public async Task<List<FileEntity>> GetFilesByUserIdAsync(string userId)
    {
        return await _context.Files.Where(file => file.UserId == userId).ToListAsync();
    }
}

