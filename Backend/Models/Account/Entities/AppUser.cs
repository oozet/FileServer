using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public IEnumerable<DirectoryEntity> directoryEntities { get; set; } = [];
}
