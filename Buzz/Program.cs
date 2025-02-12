using Buzz;
using Buzz.Model;
using Buzz.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithCorrelationId()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddDbContextFactory<RotationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<IAssignmentUpdateService, AssignmentUpdateService>();
builder.Services.AddTransient<IRotationService, RotationService>();
builder.Services.AddTransient<IWorkingDayCheckService, WorkingDayCheckService>();
builder.Services.AddTransient<SendToSlackService>();
builder.Services.AddTransient<QuartzService>();

// Add Quartz services
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
});

builder.Services.AddQuartzHostedService();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure and schedule jobs with QuartzService
using (var scope = app.Services.CreateScope())
{
    var quartzService = scope.ServiceProvider.GetRequiredService<QuartzService>();
    await quartzService.ConfigureJobsAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
