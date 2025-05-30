public class ApiError
{
    public string Message { get; set; } = string.Empty;

    public ApiError(string message)
    {
        Message = message;
    }
}