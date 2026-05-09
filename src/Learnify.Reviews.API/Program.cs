using Microsoft.AspNetCore.HttpOverrides;
using Learnify.Core.Core;
using Learnify.Reviews.API.Application;
using Learnify.Reviews.API.Storage;
using Learnify.Reviews.API.DbContexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options => { options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost; options.KnownProxies.Clear(); options.KnownNetworks.Clear(); });

// Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ReviewsDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IReviewStore, ReviewStore>();
builder.Services.AddScoped<IReviewModerator, ReviewModerator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Use shared JWT auth setup (same as all other services)
builder.Services.AddLearnifyJwtAuth(builder.Configuration);

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseForwardedHeaders();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReviewsDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (true) // Enabled for OpenShift
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
