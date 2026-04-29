using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Learnify.Core.Core;
public static class JwtBearerSetup
{
    public static IServiceCollection AddLearnifyJwtAuth(this IServiceCollection services,IConfiguration config)
    {
        var rawKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        var authBuilder = services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey)),
                    ValidateIssuer = true,
                    ValidIssuer = config["Jwt:Issuer"] ?? "learnify",
                    ValidateAudience = true,
                    ValidAudience = config["Jwt:Audience"] ?? "learnify-users",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        var googleClientId = config["Google:ClientId"];
        var googleClientSecret = config["Google:ClientSecret"];

        if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
        {
            authBuilder.AddGoogle(opts =>
            {
                opts.ClientId = googleClientId;
                opts.ClientSecret = googleClientSecret;
            });
        }

        services.AddAuthorization();
        
        services.AddHttpClient<IAuditLogger, AuditLogger>();
        
        return services;
    }
}
