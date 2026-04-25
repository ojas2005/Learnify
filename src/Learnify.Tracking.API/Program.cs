using Learnify.Core.Core;
using Learnify.Tracking.API.Application;
using Learnify.Tracking.API.DbContexts;
using Learnify.Tracking.API.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TrackingDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("TrackingDb")));

builder.Services.AddLearnifyJwtAuth(builder.Configuration);
builder.Services.AddScoped<IWatchRecordStore, WatchRecordStore>();
builder.Services.AddScoped<ILearningJournal, LearningJournal>();

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
