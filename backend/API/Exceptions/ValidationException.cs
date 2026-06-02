using Shared.Responses;
using System.Collections.Generic;

namespace API.Exceptions;

public class ValidationException : ApiException
{
    public ValidationException(string message, List<ErrorDetail> errors) : base(message, 400, errors) { }
}
