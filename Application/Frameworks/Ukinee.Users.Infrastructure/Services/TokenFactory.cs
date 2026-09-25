using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Ukinee.Users.Domain;
using Ukinee.Users.Domain.Contracts;

namespace Ukinee.Users.Infrastructure.Services;

public class TokenFactory : IUserTokenFactory
{
    private readonly IOptions<TokenOptions> _options;
    private readonly TimeProvider _timeProvider;

    private readonly JsonWebTokenHandler _handler = new JsonWebTokenHandler();
    private readonly SigningCredentials _signingCredentials;

    public TokenFactory(IOptions<TokenOptions> options, TimeProvider timeProvider)
    {
        _options = options;
        _timeProvider = timeProvider;

        var keyBytes = Encoding.UTF8.GetBytes(_options.Value.SecretKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);

        _signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256Signature
        );

        _handler.SetDefaultTimesOnTokenCreation = false;
    }

    public string Generate(User user)
    {
        var claims = new List<Claim> {
            new Claim(JwtRegisteredClaimNames.Sub, user.Identifier.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Account.Username),
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims);

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var descriptor = new SecurityTokenDescriptor {
            Issuer = _options.Value.Issuer,
            Audience = _options.Value.Audience,
            Subject = identity,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.Add(_options.Value.AccessTokenLifetime),
            SigningCredentials = _signingCredentials,
        };

        return _handler.CreateToken(descriptor);
    }
}
