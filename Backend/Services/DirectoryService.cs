public interface IDirectoryService
{
    Task<DirectoryEntity> GetOrCreateDirectoryAsync(
        string name,
        int? parentDirectoryId,
        string userId
    );
    Task<List<DirectoryEntity>> GetDirectoriesByUserIdAsync(string userId);
    Task<DirectoryEntity> CreateRootAsync(string userId);
    Task<DirectoryEntity> CreateDirectoryAsync(CreateDirectoryRequest request, string userId);
    Task DeleteDirectoryAsync(int id, string userId);
}

public class DirectoryService : IDirectoryService
{
    private readonly IDirectoryRepository _directoryRepository;

    public DirectoryService(
        IDirectoryRepository directoryRepository
    )
    {
        _directoryRepository = directoryRepository;
    }

    public async Task<DirectoryEntity> GetOrCreateDirectoryAsync(
        string name,
        int? parentDirectoryId,
        string userId
    )
    {
        var existingDirectory = await _directoryRepository.GetByNameAndParentAsync(
            name,
            parentDirectoryId,
            userId
        );
        if (existingDirectory != null)
        {
            return existingDirectory;
        }

        var newDirectory = new DirectoryEntity
        {
            Name = name,
            ParentDirectoryId = parentDirectoryId,
            UserId = userId,
        };

        await _directoryRepository.AddAsync(newDirectory);
        return newDirectory;
    }

    public async Task<List<DirectoryEntity>> GetDirectoriesByUserIdAsync(string userId)
    {
        return await _directoryRepository.GetAllByUserId(userId);
    }

    public async Task<DirectoryEntity> CreateRootAsync(string userId)
    {
        var root = new DirectoryEntity { Name = "Root", UserId = userId };

        await _directoryRepository.AddAsync(root);
        return root;
    }

    public async Task<DirectoryEntity> CreateDirectoryAsync(
        CreateDirectoryRequest request,
        string userId
    )
    {
        var directory = new DirectoryEntity
        {
            Name = request.Name,
            ParentDirectoryId = request.ParentDirectoryId,
            UserId = userId,
        };

        await _directoryRepository.AddAsync(directory);
        return directory;
    }

    public async Task DeleteDirectoryAsync(int id, string userId)
    {
        var directory = await _directoryRepository.GetWithRelationsAsync(id) ?? throw new NotFoundException($"No directory with id: {id} found.");

        if (directory.UserId != userId)
        {
            throw new UnauthorizedAccessException($"User {userId} tried to delete directory {id}");
        }
        if (directory.ParentDirectoryId == null)
        {
            throw new NotAllowedException("Cannot delete root.");
        }
        if (directory.Files.Count > 0 || directory.ChildDirectories.Count > 0)
        {
            throw new NotAllowedException("Cannot delete a non empty directory.");
        }

        _directoryRepository.Remove(directory);
    }
}