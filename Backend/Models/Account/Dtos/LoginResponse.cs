public class LoginResponse
{
    public required string AccessToken { get; set; }
    public required UserDto User { get; set; }
}
