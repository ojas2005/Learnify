using Microsoft.AspNetCore.HttpOverrides;
using Learnify.Core.Core;
using Learnify.Tracking.API.Application;
using Learnify.Tracking.API.DbContexts;
using Learnify.Tracking.API.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options => { options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost; options.KnownProxies.Clear(); options.KnownNetworks.Clear(); });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<TrackingDbContext>(opts =>
    opts.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddLearnifyJwtAuth(builder.Configuration);
builder.Services.AddScoped<IWatchRecordStore, WatchRecordStore>();
builder.Services.AddScoped<ILearningJournal, LearningJournal>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseForwardedHeaders();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TrackingDbContext>();
    db.Database.EnsureCreated();
}
app.UseSwagger(); app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
