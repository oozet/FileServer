public class DirectoryDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int? ParentDirectoryId { get; set; }
}
