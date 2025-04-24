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
}

public class DirectoryService : IDirectoryService
{
    private readonly IDirectoryRepository _directoryRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public DirectoryService(
        IDirectoryRepository directoryRepository,
        ITokenService tokenService,
        ILogger<AuthController> logger
    )
    {
        _directoryRepository = directoryRepository;
        _tokenService = tokenService;
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

        //await _directoryRepository.AddAsync(newDirectory);
        return newDirectory;
    }
}
