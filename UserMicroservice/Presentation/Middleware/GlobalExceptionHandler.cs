using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UserMicroservice.Domain.Exceptions;

namespace UserMicroservice.Presentation.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Default values
        var statusCode = StatusCodes.Status500InternalServerError;
        var title = "Server error";
        var detail = exception.Message;
        var additionalData = new Dictionary<string, object>();

        if (exception is AppException customException)
        {
            logger.LogError(exception, "Custom exception occurred: {Message}", exception.Message);

            switch (exception)
            {
                case NotFoundException ex:
                    statusCode = StatusCodes.Status404NotFound;
                    title = "NotFoundException";
                    detail = ex.Message;
                    break;
                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    title = "CustomExceptionNotImplemented";
                    detail = customException.Message;
                    break;
            }
        }
        else
        {
            logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Extensions = additionalData!
        };
        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}