using Learnify.Core.Core;
using Learnify.Courses.API.Application;
using Learnify.Courses.API.DbContexts;
using Learnify.Courses.API.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CourseDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("CoursesDb")));

builder.Services.AddLearnifyJwtAuth(builder.Configuration);

builder.Services.AddScoped<ICourseStore, CourseStore>();
builder.Services.AddScoped<ICourseCatalog, CourseCatalog>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
