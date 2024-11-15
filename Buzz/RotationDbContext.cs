using Buzz.Model;
using Microsoft.EntityFrameworkCore;
using Task = Buzz.Model.Task;

namespace Buzz;

public class RotationDbContext : DbContext
{
    public DbSet<TaskAssignment> TaskAssignments { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<Member> Members { get; set; }


    public RotationDbContext(DbContextOptions<RotationDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Username=postgres;Password=password;Database=postgres");
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

        modelBuilder.Entity<Task>(entity =>
        {
            entity.ToTable("tasks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TaskName).HasColumnName("task_name");
            entity.Property(e => e.PeriodType).HasColumnName("period_type");
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
    