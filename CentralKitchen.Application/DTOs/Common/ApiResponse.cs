namespace CentralKitchen.Application.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = null!;

    public ApiResponse()
    {
    }

    public ApiResponse(bool success, T? data, string message)
    {
        Success = success;
        Data = data;
        Message = message;
    }

    public static ApiResponse<T> Ok(T? data, string message = "Success")
    {
        return new ApiResponse<T>(true, data, message);
    }

    public static ApiResponse<T> Fail(string message)
    {
        return new ApiResponse<T>(false, default, message);
    }
}
