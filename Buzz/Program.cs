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
    var dbContext = scope.ServiceProvider.GetRequiredService<RotationDbContext>();
    
    try 
    {
        Log.Information("正在检查数据库连接...");
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (!canConnect)
        {
            Log.Error("数据库连接失败");
            throw new Exception("数据库连接失败");
        }
        Log.Information("数据库连接成功");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "数据库连接失败: {ErrorMessage}", ex.Message);
        throw; // 如果数据库连接失败，应用程序应该停止启动
    }
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
