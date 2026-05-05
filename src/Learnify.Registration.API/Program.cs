using Learnify.Core.Core;
using Learnify.Registration.API.Application;
using Learnify.Registration.API.DbContexts;
using Learnify.Registration.API.Storage;
using Microsoft.EntityFrameworkCore;

// set up the web app
var builder = WebApplication.CreateBuilder(args);

// connect to the database
builder.Services.AddDbContext<RegistrationDbContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("RegistrationDb"));
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

// start the service - drop stale shadow FK columns/constraints from the DB
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
    var sqls = new[]
    {
        "ALTER TABLE \"CourseRegistrations\" DROP CONSTRAINT IF EXISTS \"FK_CourseRegistrations_CourseOffering_CourseId\"",
        "ALTER TABLE \"CourseRegistrations\" DROP CONSTRAINT IF EXISTS \"FK_CourseRegistrations_LearnerAccount_LearnerId\"",
        "ALTER TABLE \"CourseRegistrations\" DROP CONSTRAINT IF EXISTS \"FK_CourseRegistrations_LearnerAccounts_LearnerAccountId\"",
        "ALTER TABLE \"CourseRegistrations\" DROP CONSTRAINT IF EXISTS \"FK_CourseRegistrations_LearnerAccounts_LearnerId\"",
        "ALTER TABLE \"CourseRegistrations\" DROP COLUMN IF EXISTS \"CourseOfferingId\"",
        "ALTER TABLE \"CourseRegistrations\" DROP COLUMN IF EXISTS \"LearnerAccountId\"",
    };
    foreach (var sql in sqls)
    {
        try { context.Database.ExecuteSqlRaw(sql); } catch { /* ignore */ }
    }
}

app.Run();
