using System.Net;

public class ApiResponse<T>
{
    public bool  Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public HttpStatusCode StatusCode { get; set; }

    public static ApiResponse<T> SuccessResponse(
        T data,
        string message = "Request successful",
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode
        };
    }

    public static ApiResponse<T> FailResponse(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            StatusCode = statusCode
        };
    }
}