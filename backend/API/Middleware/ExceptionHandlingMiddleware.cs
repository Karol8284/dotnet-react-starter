using System.Net;
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        ApiResponse? response;
        int statusCode;

        switch (exception)
        {
            case ApiException apiEx:
                statusCode = apiEx.StatusCode;
                response = ApiResponse.Error(statusCode, apiEx.Message, apiEx.Errors);
                _logger.LogWarning(exception, "Handled ApiException: {Message}", apiEx.Message);
                break;
            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                response = ApiResponse.Error(statusCode, "An unexpected error occurred", null);
                _logger.LogError(exception, "Unhandled exception");
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}