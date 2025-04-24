public class FileEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string ContentType { get; set; }
    public long Length { get; set; }
    public required byte[] Content { get; set; }
    public string? Directory { get; set; }
    public string? ParentDirectory { get; set; }

    public required string UserId { get; set; }
    public AppUser User { get; set; }
}
