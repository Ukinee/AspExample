using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ukinee.Infrastructure.Ddd.Common.Exceptions;
using Ukinee.Infrastructure.Ddd.Local.AccessValidation.Exceptions;

namespace Ukinee.Infrastructure.Ddd.DependencyInjection.Services;

public class DddExceptionsHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        switch (exception)
        {
            case EntityNotFoundException notFoundEx:
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await WriteProblemDetailsAsync(httpContext, "Entity Not Found", notFoundEx.Message, cancellationToken);

                return true;

            case EntityCreationAccessDeniedException accessEx:
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await WriteProblemDetailsAsync(httpContext, "Access Denied", accessEx.Message, cancellationToken);

                return true;

            case ValidationException validationEx:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await WriteValidationProblemDetailsAsync(httpContext, validationEx, cancellationToken);

                return true;

            default: return false;
        }
    }

    private static Task WriteProblemDetailsAsync(HttpContext context, string title, string detail, CancellationToken ct)
    {
        var problemDetails = new ProblemDetails {
            Status = context.Response.StatusCode,
            Title = title,
            Detail = detail,
        };

        return context.Response.WriteAsJsonAsync(problemDetails, ct);
    }

    private static Task WriteValidationProblemDetailsAsync(HttpContext context, ValidationException ex, CancellationToken ct)
    {
        var errors = ex
            .Errors
            .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "Request" : e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var validationProblemDetails = new HttpValidationProblemDetails(errors) {
            Status = context.Response.StatusCode,
            Type = "https://ietf.org",
            Title = "One or more validation errors occurred.",
            Detail = "Please refer to the errors property for additional details.",
        };

        return context.Response.WriteAsJsonAsync(validationProblemDetails, ct);
    }
}
