namespace Shared.Responses
{
    /// <summary>
    /// Details about a specific error or validation failure
    /// </summary>
    public class ErrorDetail
    {
        /// <summary>
        /// Field name that caused the error
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Error code for client-side handling
        /// </summary>
        public string? Code { get; set; }

        public ErrorDetail() { }

        public ErrorDetail(string message, string? field = null, string? code = null)
        {
            Message = message;
            Field = field;
            Code = code;
        }
    }
}