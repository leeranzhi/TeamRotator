using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamRotator.Core.Entities;

namespace TeamRotator.Infrastructure.Data;

public class RotationDbContext : DbContext
{
    private readonly ILogger<RotationDbContext>? _logger;

    public DbSet<TaskAssignment> TaskAssignments { get; set; } = null!;
    public DbSet<RotationTask> Tasks { get; set; } = null!;
    public DbSet<Member> Members { get; set; } = null!;

    public RotationDbContext(DbContextOptions<RotationDbContext> options, ILogger<RotationDbContext>? logger = null) 
        : base(options)
    {
        _logger = logger;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            _logger?.LogWarning("数据库配置未通过DI配置，使用默认连接字符串");
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=password;Database=postgres");
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskAssignment>(entity =>
        {
            entity.ToTable("task_assignments"); 
            
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.MemberId).HasColumnName("member_id");
            entity.Property(e => e.Id).HasColumnName("id");
        });

        modelBuilder.Entity<RotationTask>(entity =>
        {
            entity.ToTable("tasks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TaskName).HasColumnName("task_name");
            entity.Property(e => e.RotationRule).HasColumnName("rotation_rule");
        });
        
        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("members");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Host).HasColumnName("host");
            entity.Property(e => e.SlackId).HasColumnName("slack_id");
        });
    }
} 