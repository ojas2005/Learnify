using Learnify.Core.Core;
using Learnify.Registration.API.Application;
using Learnify.Registration.API.DbContexts;
using Learnify.Registration.API.Storage;
using Microsoft.EntityFrameworkCore;

// set up the web app
var builder = WebApplication.CreateBuilder(args);

// connect to the database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<RegistrationDbContext>(opts =>
{
    opts.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// setup security and shared services
builder.Services.AddLearnifyJwtAuth(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAuditLogger, AuditLogger>();
builder.Services.AddScoped<ISeatStore, SeatStore>();
builder.Services.AddScoped<ISeatReservation, SeatReservation>();

// register controllers and help pages
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
    db.Database.EnsureCreated();
}

// show swagger help docs if in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// send root requests to swagger help page
app.MapGet("/", () =>
{
    return Results.Redirect("/swagger");
});

app.MapControllers();

app.Run();
