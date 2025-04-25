public class DirectoryEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? ParentDirectoryId { get; set; } = null;
    public List<FileEntity> Files { get; set; } = new();

    public required string UserId { get; set; }
    public AppUser User { get; set; }
}
