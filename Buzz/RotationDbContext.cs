using Microsoft.EntityFrameworkCore;

namespace Buzz
{
    public class RotationDbContext : DbContext
    {
        public DbSet<TaskAssignment> TaskAssignments { get; set; }

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
                entity.Property(e => e.PersonName).HasColumnName("person_name");
                entity.Property(e => e.PersonId).HasColumnName("person_id");
                entity.Property(e => e.StartDate).HasColumnName("start_date");
                entity.Property(e => e.EndDate).HasColumnName("end_date");
                entity.Property(e => e.MemberId).HasColumnName("member_id");
            });
        }

    }
}