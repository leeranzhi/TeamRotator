using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;
using TeamRotator.Api.Middleware;
using TeamRotator.Core.Interfaces;
using TeamRotator.Infrastructure.Data;
using TeamRotator.Infrastructure.Jobs;
using TeamRotator.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/teamrotator-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContextFactory<RotationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add HttpClient
builder.Services.AddHttpClient();

// Register services
builder.Services.AddScoped<ITimeProvider, DefaultTimeProvider>();
builder.Services.AddScoped<IWorkingDayCheckService, WorkingDayCheckService>();
builder.Services.AddScoped<IRotationService, RotationService>();
builder.Services.AddScoped<IAssignmentUpdateService, AssignmentUpdateService>();
builder.Services.AddScoped<SendToSlackService>();

// Configure Quartz
builder.Services.AddQuartz(q =>
{
    // Register AssignmentUpdateJob
    var assignmentUpdateJobKey = new JobKey("AssignmentUpdateJob", "TeamRotator");
    q.AddJob<AssignmentUpdateJob>(opts => opts.WithIdentity(assignmentUpdateJobKey));
    q.AddTrigger(opts => opts
        .ForJob(assignmentUpdateJobKey)
        .WithIdentity("AssignmentUpdateTrigger", "TeamRotator")
        .WithCronSchedule("0 0 0 * * ?")); // Every day at midnight

    // Register SendToSlackJob
    var slackJobKey = new JobKey("SendToSlackJob", "TeamRotator");
    q.AddJob<SendToSlackJob>(opts => opts.WithIdentity(slackJobKey));
    q.AddTrigger(opts => opts
        .ForJob(slackJobKey)
        .WithIdentity("SendToSlackTrigger", "TeamRotator")
        .WithCronSchedule("0 0 8 * * ?")); // Every day at 8:00 AM
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionHandling();
app.UseAuthorization();

// Use CORS before routing
app.UseCors();

app.MapControllers();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RotationDbContext>();
    await context.Database.MigrateAsync();
}

app.Run(); 