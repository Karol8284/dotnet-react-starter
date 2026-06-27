using System.Net;
using System.Data;
using System.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Responses;
using System.Text.Json;
using API.Exceptions;

namespace API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    // Dictionary pattern (OCP - Open/Closed Principle):
    // Nowy typ wyjątku? Dodaj wpis tutaj - bez modyfikacji logiki HandleExceptionAsync.
    // Jeśli dokładny typ nie jest zarejestrowany, ResolveException() przejdzie do
    // typu bazowego w górę hierarchii aż do object (wtedy fallback → 500).
    private static readonly Dictionary<Type, (int StatusCode, string Message, bool IsWarning)> ExceptionMap = new()
    {
        // 400 Bad Request
        [typeof(ArgumentException)]             = (400, "Invalid argument",               true),
        [typeof(ArgumentNullException)]         = (400, "Required value is missing",      true),
        [typeof(ArgumentOutOfRangeException)]   = (400, "Value is out of allowed range",  true),
        [typeof(FormatException)]               = (400, "Invalid data format",            true),
        [typeof(InvalidCastException)]          = (400, "Invalid type conversion",        true),
        [typeof(InvalidDataException)]          = (400, "Invalid input data",             true),
        [typeof(NotSupportedException)]         = (400, "Operation is not supported",     true),
        [typeof(OverflowException)]             = (400, "Numeric value overflow",         true),

        // 401 Unauthorized
        [typeof(UnauthorizedAccessException)]   = (401, "Unauthorized",                   true),
        [typeof(SecurityException)]             = (401, "Access denied",                  true),

        // 404 Not Found
        [typeof(KeyNotFoundException)]          = (404, "Resource not found",             true),
        [typeof(FileNotFoundException)]         = (404, "File not found",                 true),
        [typeof(DirectoryNotFoundException)]    = (404, "Directory not found",            true),

        // 408 Request Timeout
        [typeof(TimeoutException)]              = (408, "Request timed out",              true),

        // 409 Conflict
        [typeof(InvalidOperationException)]     = (409, "Operation conflict",             true),
        [typeof(DuplicateNameException)]        = (409, "Duplicate resource",             true),

        // 422 Unprocessable Entity
        [typeof(DBConcurrencyException)]        = (422, "Data concurrency conflict",      true),

        // 500 Internal Server Error
        [typeof(NullReferenceException)]        = (500, "An unexpected error occurred",   false),
        [typeof(IndexOutOfRangeException)]      = (500, "An unexpected error occurred",   false),
        [typeof(OutOfMemoryException)]          = (500, "Server is out of resources",     false),
        [typeof(ArithmeticException)]           = (500, "An unexpected error occurred",   false),
        [typeof(NotImplementedException)]       = (500, "Feature is not implemented",     false),
        [typeof(DivideByZeroException)]         = (500, "An unexpected error occurred",   false),

        // 503 Service Unavailable
        [typeof(TaskCanceledException)]         = (503, "Service temporarily unavailable", false),
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Klient rozłączył się zanim dostał odpowiedź - nie ma komu odesłać response'u
            _logger.LogInformation("Request cancelled by client: {Path}", context.Request.Path);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Jeśli response już się rozpoczął (np. streaming), nie możemy zmienić nagłówków
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response already started, cannot handle exception for {Path}", context.Request.Path);
            return;
        }

        // ApiException i jej podklasy (NotFoundException, ValidationException itp.) mają priorytet -
        // niosą własny StatusCode i Errors ustawione przez autora kodu
        if (exception is ApiException apiEx)
        {
            _logger.LogWarning(exception, "ApiException [{StatusCode}]: {Message}", apiEx.StatusCode, apiEx.Message);
            await WriteResponseAsync(context, apiEx.StatusCode, ApiResponse.Error(apiEx.StatusCode, apiEx.Message, apiEx.Errors));
            return;
        }

        var (statusCode, message, isWarning) = ResolveException(exception);

        if (isWarning)
            _logger.LogWarning(exception, "Handled exception [{StatusCode}] {Type}: {Message}", statusCode, exception.GetType().Name, exception.Message);
        else
            _logger.LogError(exception, "Unhandled exception [{StatusCode}] {Type}: {Message}", statusCode, exception.GetType().Name, exception.Message);

        await WriteResponseAsync(context, statusCode, ApiResponse.Error(statusCode, message));
    }

    // Przechodzi w górę hierarchii dziedziczenia aż znajdzie wpis w ExceptionMap.
    // Przykład: MyCustomTimeoutException : TimeoutException → automatycznie dostanie 408.
    private static (int StatusCode, string Message, bool IsWarning) ResolveException(Exception exception)
    {
        var type = exception.GetType();

        while (type != null && type != typeof(object))
        {
            if (ExceptionMap.TryGetValue(type, out var mapped))
                return mapped;

            type = type.BaseType;
        }

        return ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred", false);
    }

    private static Task WriteResponseAsync(HttpContext context, int statusCode, ApiResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}