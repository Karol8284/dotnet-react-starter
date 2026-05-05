using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters
{
    /// <summary>
    /// Validation Filter - Checks if ModelState is valid
    /// </summary>
    public class ValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        k => k.Key,
                        v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                context.Result = new BadRequestObjectResult(new
                {
                    statusCode = 400,
                    message = "Validation failed",
                    errors = errors
                });
            }

            base.OnActionExecuting(context);
        }
    }
}
