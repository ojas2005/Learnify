using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.AspNetCore.Http;

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
                opts.DefaultSignInScheme = "ExternalCookie";
            })
            .AddCookie("ExternalCookie", opts => 
            {
                opts.Cookie.SameSite = SameSiteMode.Lax;
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
                opts.CallbackPath = "/signin-google";
                opts.SaveTokens = true;
                // Fix for "Correlation failed" on localhost through reverse proxy
                opts.CorrelationCookie.SameSite = SameSiteMode.Lax;
                opts.CorrelationCookie.HttpOnly = true;
                opts.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                opts.CorrelationCookie.Path = "/";
            });
        }

        services.AddAuthorization();
        
        services.AddHttpClient<IAuditLogger, AuditLogger>();
        
        return services;
    }
}
