using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class DirectoryEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }
    public int? ParentDirectoryId { get; set; } = null;

    public ICollection<FileEntity> Files { get; set; } = [];

    [ForeignKey(nameof(User))]
    public required string UserId { get; set; }
    public AppUser User { get; set; }
}
