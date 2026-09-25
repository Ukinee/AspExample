using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ukinee.Users.Domain;

namespace Ukinee.Users.Startup;

public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly IOptions<TokenOptions> _tokenOptions;

    public ConfigureJwtBearerOptions(IOptions<TokenOptions> tokenOptions)
    {
        _tokenOptions = tokenOptions;
    }

    public void Configure(JwtBearerOptions options)
    {
        var optionsValue = _tokenOptions.Value;

        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidIssuer = optionsValue.Issuer,

            ValidateAudience = true,
            ValidAudience = optionsValue.Audience,

            ValidateLifetime = true,
            ClockSkew = optionsValue.ClockSkew,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(optionsValue.SecretKey)),

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name,
        };
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }
}
