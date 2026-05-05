using Shared.Responses;
using System.Net;

namespace API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {ExceptionMessage}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handles the exception and returns appropriate response
        /// </summary>
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                // Validation exceptions
                ArgumentNullException ex => new
                {
                    statusCode = HttpStatusCode.BadRequest,
                    message = $"Required argument is null: {ex.ParamName}",
                    code = "ARGUMENT_NULL"
                },
                ArgumentException ex => new
                {
                    statusCode = HttpStatusCode.BadRequest,
                    message = $"Invalid argument: {ex.Message}",
                    code = "ARGUMENT_INVALID"
                },

                // Not found exceptions
                KeyNotFoundException ex => new
                {
                    statusCode = HttpStatusCode.NotFound,
                    message = ex.Message ?? "Resource not found",
                    code = "NOT_FOUND"
                },

                // Unauthorized exceptions
                UnauthorizedAccessException ex => new
                {
                    statusCode = HttpStatusCode.Unauthorized,
                    message = "Unauthorized access",
                    code = "UNAUTHORIZED"
                },

                // Generic catch-all
                _ => new
                {
                    statusCode = HttpStatusCode.InternalServerError,
                    message = "An internal server error occurred",
                    code = "INTERNAL_SERVER_ERROR"
                }
            };

            context.Response.StatusCode = (int)response.statusCode;

            var apiResponse = new ApiResponse(
                (int)response.statusCode,
                response.message,
                new List<ErrorDetail>
                {
                new ErrorDetail(response.message, code: response.code)
                }
            );

            return context.Response.WriteAsJsonAsync(apiResponse);
        }
    }

    /// <summary>
    /// Custom exceptions for API
    /// </summary>
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }
        public List<ErrorDetail>? ErrorDetails { get; }

        public ApiException(string message, int statusCode = 400, string errorCode = "API_ERROR", List<ErrorDetail>? errorDetails = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            ErrorDetails = errorDetails;
        }
    }

    /// <summary>
    /// Thrown when a resource is not found
    /// </summary>
    public class ResourceNotFoundException : ApiException
    {
        public ResourceNotFoundException(string resourceName, object resourceId)
            : base($"{resourceName} with id '{resourceId}' not found", 404, "RESOURCE_NOT_FOUND")
        {
        }

        public ResourceNotFoundException(string message)
            : base(message, 404, "RESOURCE_NOT_FOUND")
        {
        }
    }

    /// <summary>
    /// Thrown when a business rule is violated
    /// </summary>
    public class BusinessRuleException : ApiException
    {
        public BusinessRuleException(string message, string errorCode = "BUSINESS_RULE_VIOLATION")
            : base(message, 400, errorCode)
        {
        }
    }

    /// <summary>
    /// Thrown when validation fails
    /// </summary>
    public class ValidationException : ApiException
    {
        public ValidationException(string message, List<ErrorDetail>? errorDetails = null)
            : base(message, 400, "VALIDATION_ERROR", errorDetails)
        {
        }

        public ValidationException(List<ErrorDetail> errorDetails)
            : base("One or more validation errors occurred", 400, "VALIDATION_ERROR", errorDetails)
        {
        }
    }
}