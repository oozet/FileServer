public class LoginResult
{
    public required bool Success { get; set; }
    public AppUser? User { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
