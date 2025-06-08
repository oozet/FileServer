public interface IFileService
{
    Task SaveFileAsync(FileEntity fileEntity);
    Task DeleteFileAsync(string id, string userId);
    Task<List<FileInformationDto>> GetFilesByUserIdAsync(string userId);
    Task<FileEntity> GetFileAsync(string userId, string fileId);
    Task<Dictionary<FileEntity, string>> GetAllFiles();
}

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly IDirectoryRepository _directoryRepository;

    public FileService(
        IFileRepository fileRepository,
        IDirectoryRepository directoryRepository
    )
    {
        _fileRepository = fileRepository;
        _directoryRepository = directoryRepository;
    }

    public Task<Dictionary<FileEntity, string>> GetAllFiles()
    {
        throw new NotImplementedException();
    }

    public async Task<FileEntity> GetFileAsync(string userId, string fileId)
    {
        var file =
            await _fileRepository.GetAsync(fileId)
            ?? throw new NotFoundException("File not found.");
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
        var dir =
            await _directoryRepository.GetAsync(fileEntity.DirectoryId)
            ?? throw new NotFoundException("Directory not found.");
        dir.Files.Add(fileEntity);
        await _fileRepository.AddAsync(fileEntity);
    }

    public async Task DeleteFileAsync(string id, string userId)
    {
        var fileToDelete = await _fileRepository.GetAsync(id);
        if (fileToDelete == null)
        {
            throw new NotFoundException("File not found.");
        }
        if (fileToDelete.UserId != userId)
        {
            throw new UnauthorizedAccessException("User id does not match file owner.");
        }

        _fileRepository.Remove(fileToDelete);
    }
}
