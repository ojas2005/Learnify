using Learnify.Core.Core;
using Learnify.Identity.API.Application;
using Learnify.Identity.API.DbContexts;
using Learnify.Identity.API.Security;
using Learnify.Identity.API.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Learnify.Core.Domain;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownProxies.Clear();
    options.KnownNetworks.Clear();
});

//Data
builder.Services.AddDbContext<IdentityDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("IdentityDb")));

// Data Protection (required for OAuth correlation through reverse proxy)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "DataProtectionKeys")))
    .SetApplicationName("LearnifyIdentity");

// Cookie Policy (required for OAuth correlation through reverse proxy)
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.SameAsRequest;
    options.OnAppendCookie = cookieContext =>
    {
        if (cookieContext.CookieOptions.SameSite == SameSiteMode.None)
        {
            cookieContext.CookieOptions.SameSite = SameSiteMode.Unspecified;
        }
    };
    options.OnDeleteCookie = cookieContext =>
    {
        if (cookieContext.CookieOptions.SameSite == SameSiteMode.None)
        {
            cookieContext.CookieOptions.SameSite = SameSiteMode.Unspecified;
        }
    };
});

//Auth
builder.Services.AddLearnifyJwtAuth(builder.Configuration);

//Domain services
builder.Services.AddScoped<ILearnerStore,LearnerStore>();
builder.Services.AddSingleton<TokenMinter>();
builder.Services.AddScoped<IIdentityBroker,IdentityBroker>();
builder.Services.AddScoped<IPasswordHasher<LearnerAccount>,PasswordHasher<LearnerAccount>>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();

//Infrastructure
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.AllowTrailingCommas = true;
        options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",new OpenApiInfo { Title = "Learnify Identity API",Version = "v1" });

    c.AddSecurityDefinition("Bearer",new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme,example:- \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();
app.UseForwardedHeaders();
app.UseCookiePolicy();
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json","Learnify Identity API");
    c.RoutePrefix = string.Empty; //serves swagger ui at root,port-5005
});

// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
