using Learnify.Core.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Learnify.Identity.API.Security;

//responsible for creating and validating JWT access tokens.
//extracted from IdentityBroker because token mechanics change independently of business auth logic.
//this allows swapping algorithms without touching login flows.
public sealed class TokenMinter
{
    private readonly SymmetricSecurityKey _signingKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly TimeSpan _tokenLifetime;
    private readonly TokenValidationParameters _validationParams;

    public TokenMinter(IConfiguration config)
    {
        var rawKey = config["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey));
        _issuer = config["Jwt:Issuer"] ?? "learnify";
        _audience = config["Jwt:Audience"] ?? "learnify-users";

        // Default to 24 hours; override via config if needed
        _tokenLifetime = TimeSpan.FromHours(
            double.TryParse(config["Jwt:ExpiryHours"],out var hours) ? hours : 24);

        _validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true,
            //zero skew to keep expiry strict,don't silently extend sessions
            ClockSkew = TimeSpan.Zero
        };
    }

//mints a signed JWT for the given account.
//claims encode identity,role,and display info needed by services.
    public string IssueToken(LearnerAccount account)
    {
        var claims = BuildClaimsFor(account);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(_tokenLifetime),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(_signingKey,SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }

//returns true only if the token is properly signed, not expired, and from our issuer.
    public bool ValidateToken(string rawToken)
    {
        try
        {
            new JwtSecurityTokenHandler()
                .ValidateToken(rawToken,_validationParams,out _);
            return true;
        }
        catch
        {
            //any validation failure(expired,tampered,wrong issuer) then false
            return false;
        }
    }

    private static Claim[] BuildClaimsFor(LearnerAccount account)
    {
        return
        [
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, account.EmailAddress),
            new(JwtRegisteredClaimNames.Name, account.DisplayName),
            new(ClaimTypes.Role, account.Role.ToString()),
            //unique token id helps with potential revocation list lookups later
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        ];
    }
}
