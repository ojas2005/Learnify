using Learnify.Core.Core;
using Learnify.Exams.API.Application;
using Learnify.Exams.API.DbContexts;
using Learnify.Exams.API.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ExamsDbContext>(opts =>
    opts.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddLearnifyJwtAuth(builder.Configuration);
builder.Services.AddScoped<IExamStore, ExamStore>();
builder.Services.AddScoped<IAttemptStore, AttemptStore>();
builder.Services.AddScoped<IExamEngine, ExamEngine>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ExamsDbContext>();
    db.Database.EnsureCreated();
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Learnify Exams API V1");
    c.RoutePrefix = string.Empty;
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
