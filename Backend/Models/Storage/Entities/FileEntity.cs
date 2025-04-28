using System.ComponentModel.DataAnnotations;

public class FileEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public required string Name { get; set; }
    public required string ContentType { get; set; }
    public long Length { get; set; }

    [Required]
    public required byte[] Content { get; set; }

    [Required]
    public required int DirectoryId { get; set; }
    public DirectoryEntity Directory { get; set; }

    [Required]
    public required string UserId { get; set; }
    public AppUser User { get; set; }
}
