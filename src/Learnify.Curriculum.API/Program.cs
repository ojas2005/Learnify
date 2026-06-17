using Microsoft.AspNetCore.HttpOverrides;
// learnify curriculum api entry point
using Learnify.Core.Core;
using Learnify.Curriculum.API.Application;
using Learnify.Curriculum.API.DbContexts;
using Learnify.Curriculum.API.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options => { options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost; options.KnownProxies.Clear(); options.KnownNetworks.Clear(); });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CurriculumDbContext>(opts =>
    opts.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddLearnifyJwtAuth(builder.Configuration);
builder.Services.AddScoped<ISyllabusStore, SyllabusStore>();
builder.Services.AddScoped<ICurriculumBuilder, CurriculumBuilder>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseForwardedHeaders();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CurriculumDbContext>();
    db.Database.EnsureCreated();
}
if (true) // Enabled for OpenShift
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Learnify.Curriculum.API v1");
        c.RoutePrefix = string.Empty;
    });
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
