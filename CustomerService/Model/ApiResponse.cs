namespace CustomerService.Models;

public record ApiResponse<T>(
    int Status,
    string Message,
    T? Data,
    object? Errors,
    DateTimeOffset Timestamp
)
{
    public static ApiResponse<T> Success(T? data, string message = "Success") =>
        new(200, message, data, null, DateTimeOffset.UtcNow);

    // All Fail
    public static ApiResponse<T> Fail(int status, string message, object? errors = null) =>
        new(status, message, default, errors, DateTimeOffset.UtcNow);
    
    // 400 - Bad Request
    public static ApiResponse<T> BadRequest(string message = "Bad Request", object? errors = null) =>
        new(400, message, default, errors, DateTimeOffset.UtcNow);

    // 401 - Unauthorized
    public static ApiResponse<T> Unauthorized(string message = "Unauthorized") =>
        new(401, message, default, null, DateTimeOffset.UtcNow);

    // 403 - Forbidden
    public static ApiResponse<T> Forbidden(string message = "Forbidden") =>
        new(403, message, default, null, DateTimeOffset.UtcNow);

    // 404 - Not Found
    public static ApiResponse<T> NotFound(string message = "Not Found") =>
        new(404, message, default, null, DateTimeOffset.UtcNow);
    
    // 409 - Duplicate Name
    public static ApiResponse<T> Duplicate(string message = "Duplicate") =>
        new(409, message, default, null, DateTimeOffset.UtcNow);

    // 500 - Internal Server Error
    public static ApiResponse<T> InternalError(string message = "Internal Server Error", object? errors = null) =>
        new(500, message, default, errors, DateTimeOffset.UtcNow);
}