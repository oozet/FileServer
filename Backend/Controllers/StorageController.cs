using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                return BadRequest(new ApiError("No file uploaded."));
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
                return Ok();
            }
            if (savingErrors.Count == files.Count())
            {
                return BadRequest(new ApiError("Unable to save files."));
            }
            string failedFiles = string.Join(", ", savingErrors);
            return StatusCode(207, new ApiError($"Unable to save some files: {failedFiles}"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Error retrieving user id from claims");
            return Unauthorized(new ApiError("Invalid user."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Server error in UploadFiles");
            return StatusCode(500, new ApiError("Unexpected server error."));
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

            DirectoryEntity directory =
                await _directoryService.CreateDirectoryAsync(request, userId)
                ?? throw new NullReferenceException();

            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Error retrieving user id from claims");
            return Unauthorized(new ApiError("Invalid user."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating directory.");
            return BadRequest(new ApiError("Unable to create directory"));
        }
    }

    [Authorize]
    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadFile(string id)
    {
        try
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User Id cannot be null.");

            var file = await _fileService.GetFileAsync(userId, id);

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
            _logger.LogError(
                ex,
                "Authorization error occurred while downloading file with ID {id} for user {UserName}.",
                id,
                User?.Identity?.Name ?? "Unknown user"
            );
            return Unauthorized(new ApiError("Invalid user."));
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(
                ex,
                "Authorization error occurred while downloading file with ID {id} for user {UserName}.",
                id,
                User?.Identity?.Name ?? "Unknown user"
            );
            return NotFound(new ApiError(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Server error.");
            return StatusCode(500, new ApiError("Unexpected server error."));
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
            if (directoryTree.Count == 0)
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
        catch (NotFoundException ex)
        {
            return NotFound(new ApiError(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Error retrieving user id from claims");
            return Unauthorized(new ApiError("Invalid user."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Server error.");
            return StatusCode(500, new ApiError("Unexpected error."));
        }
    }

    [Authorize]
    [HttpDelete("delete-directory/{id}")]
    public async Task<IActionResult> DeleteDirectory(int id)
    {
        try
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User Id cannot be null.");

            await _directoryService.DeleteDirectoryAsync(id, userId);

            return NoContent();
        }
        catch (NotAllowedException ex)
        {
            return BadRequest(new ApiError(ex.Message));
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Error while trying to delete directory.");
            return NotFound(new ApiError(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Error retrieving user id from claims");
            return Unauthorized(new ApiError("Invalid user"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Server error.");
            return StatusCode(500, new ApiError("Unexpected error."));
        }
    }


    [Authorize]
    [HttpDelete("delete-file/{id}")]
    public async Task<IActionResult> DeleteFile(string id)
    {
        try
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User Id cannot be null.");

            await _fileService.DeleteFileAsync(id, userId);

            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "Error while trying to delete file with id: " + id);
            return NotFound(new ApiError(ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Error retrieving user id from claims for id: " + id);
            return Unauthorized(new ApiError("Invalid user."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Server error.");
            return StatusCode(500, new ApiError("Unexpected error."));
        }
    }
}
