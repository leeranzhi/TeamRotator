using Buzz;
using Buzz.Jobs;
using Buzz.Model;
using Buzz.Services;
using Buzz.Utilities;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContextFactory<RotationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<RotationService>();
builder.Services.AddScoped<AssignmentUpdateService>();
builder.Services.AddTransient<SendToSlackService>();
builder.Services.AddTransient<IAssignmentUpdateService, AssignmentUpdateService>();

DateTime todayDate = DateTime.Today;
if (WorkingDayCheck.IsWorkingDay(todayDate))
{
    builder.Services.AddQuartz(q =>
    {
        q.UseMicrosoftDependencyInjectionJobFactory();

        var assignmentJobKey = new JobKey("AssignmentUpdateJob");
        q.AddJob<AssignmentUpdateJob>(opts => opts.WithIdentity(assignmentJobKey));
        q.AddTrigger(opts => opts
            .ForJob(assignmentJobKey)
            .WithIdentity("AssignmentUpdateJob-trigger")
            .WithCronSchedule("0 0 0 * * ?")); 

        var slackJobKey = new JobKey("SendToSlackJob");
        q.AddJob<SendToSlackJob>(opts => opts.WithIdentity(slackJobKey));
        q.AddTrigger(opts => opts
            .ForJob(slackJobKey)
            .WithIdentity("SendToSlackJob-trigger")
            .WithCronSchedule("0 0 9 * * ?")); 
    });
}

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();