using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamRotator.Core.Entities;
using Task = TeamRotator.Core.Entities.Task;

namespace TeamRotator.Infrastructure.Data;

public class RotationDbContext : DbContext
{
    private readonly ILogger<RotationDbContext>? _logger;

    public DbSet<TaskAssignment> TaskAssignments { get; set; } = null!;
    public DbSet<Task> Tasks { get; set; } = null!;
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
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<TaskAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne<Task>()
                .WithMany()
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Member>()
                .WithMany()
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
} 