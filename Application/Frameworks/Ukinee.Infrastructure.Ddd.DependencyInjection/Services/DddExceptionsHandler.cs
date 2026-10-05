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

            default: return false;
        }
    }

    private static Task WriteProblemDetailsAsync(HttpContext context, string title, string detail, CancellationToken ct)
    {
        var problemDetails = new ProblemDetails {
            Status = context.Response.StatusCode,
            Title = title,
            Detail = detail
        };

        return context.Response.WriteAsJsonAsync(problemDetails, ct);
    }
}
