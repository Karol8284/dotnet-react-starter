namespace Shared.Responses;

/// <summary>
/// Standardowa odpowiedź API dla wszystkich endpointów
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Czy operacja się powiodła
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Wiadomość dla klienta
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Dane zwrócone z API
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Błędy (jeśli było)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Konstruktor dla sukcesu
    /// </summary>
    public static ApiResponse<T> SuccessResult(T? data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Konstruktor dla błędu
    /// </summary>
    public static ApiResponse<T> ErrorResult(string message, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
