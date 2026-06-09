using Shared.Responses;
using System.Collections.Generic;

namespace API.Exceptions;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public List<ErrorDetail>? Errors { get; }

    public ApiException(string message, int statusCode = 400, List<ErrorDetail>? errors = null) : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}
