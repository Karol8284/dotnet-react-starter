namespace Shared.Responses;

/// <summary>
/// Standardized API response wrapper for all endpoints
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// List of validation/error details
    /// </summary>
    public List<ErrorDetail>? Errors { get; set; }

    /// <summary>
    /// Timestamp of response
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Constructor for successful response
    /// </summary>
    public ApiResponse(int statusCode, string message, T? data = default)
    {
        StatusCode = statusCode;
        Message = message;
        Data = data;
    }

    /// <summary>
    /// Constructor for error response
    /// </summary>
    public ApiResponse(int statusCode, string message, List<ErrorDetail>? errors = null)
    {
        StatusCode = statusCode;
        Message = message;
        Errors = errors;
    }

    /// <summary>
    /// Success response
    /// </summary>
    public static ApiResponse<T> Success(T data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<T>(statusCode, message, data);
    }

    /// <summary>
    /// Error response
    /// </summary>
    public static ApiResponse<T> Error(int statusCode, string message, List<ErrorDetail>? errors = null)
    {
        return new ApiResponse<T>(statusCode, message, errors);
    }
}

/// <summary>
/// Non-generic version for responses without data
/// </summary>
public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ErrorDetail>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiResponse(int statusCode, string message, List<ErrorDetail>? errors = null)
    {
        StatusCode = statusCode;
        Message = message;
        Errors = errors;
    }

    public static ApiResponse Success(string message = "Success", int statusCode = 200)
    {
        return new ApiResponse(statusCode, message);
    }

    public static ApiResponse Error(int statusCode, string message, List<ErrorDetail>? errors = null)
    {
        return new ApiResponse(statusCode, message, errors);
    }
}

