using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

public interface IFileService
{
    Task SaveFileAsync(FileEntity fileEntity);
    Task<List<FileInformationDto>> GetFilesByUserIdAsync(string userId);
    Task<FileEntity> GetFileAsync(string userId, string fileId);
    Task<Dictionary<FileEntity, string>> GetAllFiles();
}

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly IDirectoryRepository _directoryRepository;
    private readonly ILogger<AuthController> _logger;

    public FileService(
        IFileRepository fileRepository,
        IDirectoryRepository directoryRepository,
        ILogger<AuthController> logger
    )
    {
        _fileRepository = fileRepository;
        _directoryRepository = directoryRepository;
        _logger = logger;
    }

    public Task<Dictionary<FileEntity, string>> GetAllFiles()
    {
        throw new NotImplementedException();
    }

    public async Task<FileEntity> GetFileAsync(string userId, string fileId)
    {
        var file =
            await _fileRepository.GetAsync(fileId)
            ?? throw new NullReferenceException("File not found.");
        if (file.UserId != userId)
            throw new UnauthorizedAccessException(
                "Attempted to access a file belongin to other user."
            );

        return file;
    }

    public async Task<List<FileInformationDto>> GetFilesByUserIdAsync(string userId)
    {
        var fileList = new List<FileInformationDto>();
        var files = await _fileRepository.GetFilesByUserIdAsync(userId);
        foreach (var fileEntity in files)
        {
            fileList.Add(
                new FileInformationDto
                {
                    Id = fileEntity.Id,
                    Name = fileEntity.Name,
                    ParentDirectoryId = fileEntity.DirectoryId,
                }
            );
        }

        return fileList;
    }

    public async Task SaveFileAsync(FileEntity fileEntity)
    {
        try
        {
            var dir =
                await _directoryRepository.GetByIdAsync(fileEntity.DirectoryId)
                ?? throw new Exception("Directory not found.");
            dir.Files.Add(fileEntity);
            await _fileRepository.AddAsync(fileEntity);
            await _fileRepository.SaveChangesAsync();
        }
        catch
        {
            throw;
        }
    }
}
