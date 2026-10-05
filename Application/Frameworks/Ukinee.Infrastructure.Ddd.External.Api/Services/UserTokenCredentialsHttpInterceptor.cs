using System.Net.Http.Headers;
using Ukinee.Infrastructure.Ddd.External.Api.Utils;
using Ukinee.Infrastructure.Ddd.External.Contracts;

namespace Ukinee.Infrastructure.Ddd.External.Api.Services;

public class UserTokenCredentialsHttpInterceptor(IUserTokenStore userTokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (request.Options.TryGetValue(RequestOptionsKeys.UserContext, out var userContext))
        {
            var token = userTokenStore.GetToken(userContext);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
