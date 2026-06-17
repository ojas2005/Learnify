using Microsoft.AspNetCore.HttpOverrides;
using Learnify.Core.Core;
using Learnify.Courses.API.Application;
using Learnify.Courses.API.DbContexts;
using Learnify.Courses.API.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ForwardedHeadersOptions>(options => { options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost; options.KnownProxies.Clear(); options.KnownNetworks.Clear(); });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CourseDbContext>(opts =>
    opts.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddLearnifyJwtAuth(builder.Configuration);

builder.Services.AddScoped<ICourseStore, CourseStore>();
builder.Services.AddScoped<ICourseCatalog, CourseCatalog>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseForwardedHeaders();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CourseDbContext>();
    try {
        db.Database.EnsureCreated();
    } catch {
        // Fallback or ignore if already exists
    }
}

app.UseSwagger(); app.UseSwaggerUI();

// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
