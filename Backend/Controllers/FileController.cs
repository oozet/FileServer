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
[Route("file")]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FileController> _logger;

    public FileController(IFileService fileService, ILogger<FileController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UploadFiles([FromForm] IEnumerable<IFormFile> files)
    {
        try
        {
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
                    UserId = userId,
                };
                try
                {
                    await _fileService.SaveFile(fileEntitity);
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
    public async Task<IActionResult> GetFileList()
    {
        try
        {
            return null;
        }
        catch (NullReferenceException)
        {
            return NotFound("User doesn't exist.");
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError("An exception occurred: {Message}", ex.Message);
            return BadRequest();
        }
    }
}
