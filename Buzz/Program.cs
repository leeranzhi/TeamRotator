using Buzz;
using Buzz.Model;
using Buzz.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHttpClient();

builder.Services.AddDbContextFactory<RotationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<RotationService>();
builder.Services.AddTransient<AssignmentUpdateService>();
builder.Services.AddTransient<IAssignmentUpdateService, AssignmentUpdateService>();
builder.Services.AddTransient<SendToSlackService>();
builder.Services.AddTransient<WorkingDayCheckService>();
builder.Services.AddTransient<QuartzService>();

builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
