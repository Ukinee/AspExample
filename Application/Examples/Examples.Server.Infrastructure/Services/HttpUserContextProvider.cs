using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Ukinee.Users.Common.ValueObjects;
using Ukinee.Users.Domain.Contracts;

namespace Examples.Server.Infrastructure.Services;

//todo: refactor
public sealed class HttpUserContextProvider : IUserContextProvider
{
    private readonly IHttpContextAccessor _contextAccessor;

    public HttpUserContextProvider(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public UserContext GetActiveUserContext()
    {
        var httpContext = _contextAccessor.HttpContext
                          ?? throw new InvalidOperationException(
                              $"{nameof(IHttpContextAccessor)} does not contain an active HttpContext. " +
                              $"GetActiveUserContext() must be called within an active HTTP request."
                          );

        var principal = httpContext.User;

        if (principal.Identity?.IsAuthenticated != true)
            return UserContext.Guest;

        var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub) //todo: add static class with application constants
                  ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(sub, out var userGuid))
            throw new InvalidOperationException(
                $"Authenticated principal does not contain a valid sub claim. " +
                $"Expected a GUID under '{JwtRegisteredClaimNames.Sub}' or '{ClaimTypes.NameIdentifier}'."
            );

        var name = principal.FindFirstValue(ClaimTypes.Name);

        var roles = principal
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToImmutableArray();

        return new UserContext {
            Guid = userGuid,
            IsAuthenticated = true,
            Roles = roles,
            Name = string.IsNullOrWhiteSpace(name) ? null : name,
        };
    }
}
