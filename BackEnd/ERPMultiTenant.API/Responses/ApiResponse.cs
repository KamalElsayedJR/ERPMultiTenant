namespace ERPMultiTenant.API.Responses;

public sealed class ApiResponse<T>
{
    public ApiResponse(bool success, string message, T? data)
    {
        Success = success;
        Message = message;
        Data = data;
    }

    public bool Success { get; }
    public string Message { get; }
    public T? Data { get; }

    public static ApiResponse<T> Ok(string message, T? data = default) => new(true, message, data);
    public static ApiResponse<T> Fail(string message) => new(false, message, default);
}
