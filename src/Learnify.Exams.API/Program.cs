using Learnify.Core.Core;
using Learnify.Exams.API.Application;
using Learnify.Exams.API.DbContexts;
using Learnify.Exams.API.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ExamsDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("ExamsDb")));

builder.Services.AddLearnifyJwtAuth(builder.Configuration);
builder.Services.AddScoped<IExamStore, ExamStore>();
builder.Services.AddScoped<IAttemptStore, AttemptStore>();
builder.Services.AddScoped<IExamEngine, ExamEngine>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
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
