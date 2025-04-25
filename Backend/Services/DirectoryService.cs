using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public interface IDirectoryService
{
    Task<DirectoryEntity> GetOrCreateDirectoryAsync(
        string name,
        int? parentDirectoryId,
        string userId
    );
    Task<List<DirectoryEntity>> GetDirectoriesByUserIdAsync(string userId);
    Task<DirectoryEntity> CreateRootAsync(string userId);
}

public class DirectoryService : IDirectoryService
{
    private readonly IDirectoryRepository _directoryRepository;
    private readonly ILogger<AuthController> _logger;

    public DirectoryService(
        IDirectoryRepository directoryRepository,
        ILogger<AuthController> logger
    )
    {
        _directoryRepository = directoryRepository;
        _logger = logger;
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
        var root = new DirectoryEntity
        {
            Name = "Root",
            UserId = userId
        };

        await _directoryRepository.AddAsync(root);
        return root;
    }
}
