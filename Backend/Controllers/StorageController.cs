using System.IdentityModel.Tokens.Jwt;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("storage")]
public class StorageController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly IDirectoryService _directoryService;
    private readonly ILogger<StorageController> _logger;

    public StorageController(
        IFileService fileService,
        IDirectoryService directoryService,
        ILogger<StorageController> logger
    )
    {
        _fileService = fileService;
        _directoryService = directoryService;
        _logger = logger;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UploadFiles(
        [FromForm] IEnumerable<IFormFile> files,
        [FromForm] int directoryId
    )
    {
        try
        {
            _logger.LogInformation("Directory ID: " + directoryId);
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User Id cannot be null.");

            if (!files.Any())
            {
                return BadRequest("No file uploaded.");
            }

            var savingErrors = new List<string>();

            foreach (var file in files)
            {
                using var memoryStream = new MemoryStream();

                await file.CopyToAsync(memoryStream);
                var fileEntitity = new FileEntity
                {
                    Name = file.FileName,
                    ContentType = file.ContentType,
                    Length = file.Length,
                    Content = memoryStream.ToArray(),
                    DirectoryId = directoryId,
                    UserId = userId,
                };
                try
                {
                    await _fileService.SaveFileAsync(fileEntitity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while saving {fileName}", file.Name);
                    savingErrors.Add(file.Name);
                }
            }

            if (savingErrors.Count == 0)
            {
                return Ok("File upload was successful!");
            }
            if (savingErrors.Count == files.Count())
            {
                return BadRequest("Unable to save files.");
            }
            string failedFiles = string.Join(", ", savingErrors);
            return StatusCode(207, $"Unable to save some files: {failedFiles}");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Error retrieving user id from claims");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Server error");
            return StatusCode(500, "Unexpected error.");
        }
    }

    [Authorize]
    [HttpPost("create-dir")]
    public async Task<IActionResult> CreateDirectory([FromBody] CreateDirectoryRequest request)
    {
        try
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User Id cannot be null.");

            var directory = await _directoryService.CreateDirectoryAsync(request, userId);

            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Error retrieving user id from claims");
            return Unauthorized();
        }
        catch (Exception)
        {
            return BadRequest("Unable to create directory");
        }
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> DownloadFile(string id)
    {
        try
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User Id cannot be null.");

            var file = await _fileService.GetFileAsync(userId, id);
            if (file == null)
            {
                return NotFound();
            }

            const long StreamThreshold = 5 * 1024 * 1024;

            if (file.Length > StreamThreshold)
            {
                using var stream = new MemoryStream(file.Content);
                return File(stream, file.ContentType, file.Name);
            }
            else
            {
                return File(file.Content, file.ContentType, file.Name);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Error retrieving user id from claims");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Server error.");
            return StatusCode(500, "Unexpected error.");
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetDirectoryTree()
    {
        try
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User Id cannot be null.");

            var directoryTree = await _directoryService.GetDirectoriesByUserIdAsync(userId);
            if (directoryTree.Count() == 0)
            {
                directoryTree.Add(await _directoryService.CreateRootAsync(userId));
            }

            var directories = new List<DirectoryDto>();
            foreach (var directory in directoryTree)
            {
                directories.Add(
                    new DirectoryDto()
                    {
                        Id = directory.Id,
                        Name = directory.Name,
                        ParentDirectoryId = directory.ParentDirectoryId,
                    }
                );
            }

            var fileList = await _fileService.GetFilesByUserIdAsync(userId);
            var files = new List<FileInformationDto>();
            foreach (var file in fileList)
            {
                files.Add(
                    new FileInformationDto()
                    {
                        Id = file.Id,
                        Name = file.Name,
                        ParentDirectoryId = file.ParentDirectoryId,
                    }
                );
            }

            return Ok(new { directories, files });
        }
        catch (NullReferenceException)
        {
            return NotFound("User doesn't exist.");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Error retrieving user id from claims");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Server error.");
            return StatusCode(500, "Unexpected error.");
        }
    }
}
