public class UserDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
}
