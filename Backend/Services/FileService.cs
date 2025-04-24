using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public interface IFileService
{
    Task SaveFile(FileEntity fileEntity);
    Task<List<FileEntity>> GetFilesByUserId(string userId);
    Task<FileEntity> GetFileAsync(string userId, string fileId);
    Task<Dictionary<FileEntity, string>> GetAllFiles();
}

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly ILogger<AuthController> _logger;

    public FileService(IFileRepository fileRepository, ILogger<AuthController> logger)
    {
        _fileRepository = fileRepository;
        _logger = logger;
    }

    public Task<Dictionary<FileEntity, string>> GetAllFiles()
    {
        throw new NotImplementedException();
    }

    public Task<FileEntity> GetFileAsync(string userId, string fileId)
    {
        throw new NotImplementedException();
    }

    public Task<List<FileEntity>> GetFilesByUserId(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task SaveFile(FileEntity fileEntity)
    {
        await _fileRepository.AddFileAsync(fileEntity);
    }
}
