public class FileDto
{
    public required IFormFile File { get; set; }
    public string? DirectoryName { get; set; }
    public int? ParentDirectoryId { get; set; }
}
