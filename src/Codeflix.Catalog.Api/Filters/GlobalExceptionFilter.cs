using Codeflix.Catalog.Application.Exceptions;
using Codeflix.Catalog.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Codeflix.Catalog.Api.Filters;

public class GlobalExceptionFilter(IHostEnvironment env) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var details = new ProblemDetails();
        var exception = context.Exception;

        if (env.IsDevelopment())
            details.Extensions.Add("StackTrace", exception.StackTrace);

        if (exception is EntityValidationException)
        {
            details.Title = $"One or more validation errors occurred";
            details.Status = StatusCodes.Status422UnprocessableEntity;
            details.Type = "UnprocessableEntity";
            details.Detail = exception.Message;
        }
        else if (exception is NotFoundException)
        {
            details.Title = $"Not found";
            details.Status = StatusCodes.Status404NotFound;
            details.Type = "NotFound";
            details.Detail = exception.Message;
        }
        else
        {
            details.Title = $"An unexpected error occurred";
            details.Status = StatusCodes.Status500InternalServerError;
            details.Type = "InternalServerError";
            details.Detail = exception.Message;
        }

        context.HttpContext.Response.StatusCode = (int)details.Status;
        context.Result = new ObjectResult(details);
        context.ExceptionHandled = true;
    }
}
