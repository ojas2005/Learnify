// learnify curriculum api entry point
using Learnify.Core.Core;
using Learnify.Curriculum.API.Application;
using Learnify.Curriculum.API.DbContexts;
using Learnify.Curriculum.API.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CurriculumDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("CurriculumDb")));

builder.Services.AddLearnifyJwtAuth(builder.Configuration);
builder.Services.AddScoped<ISyllabusStore, SyllabusStore>();
builder.Services.AddScoped<ICurriculumBuilder, CurriculumBuilder>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
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

// Drop stale cross-service FK constraints
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<CurriculumDbContext>();
    try { ctx.Database.ExecuteSqlRaw("ALTER TABLE \"CurriculumLessons\" DROP CONSTRAINT IF EXISTS \"FK_CurriculumLessons_CourseOffering_CourseId\""); } catch { }
}

app.Run();
