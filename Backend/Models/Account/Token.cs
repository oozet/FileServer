public class Token
{
    public int Id { get; set; }

    public required string UserName { get; set; }
    public required string RefreshToken { get; set; }
    public required DateTime ExpiredAt { get; set; }
}
