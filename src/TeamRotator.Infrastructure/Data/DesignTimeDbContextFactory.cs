using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TeamRotator.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RotationDbContext>
{
    public RotationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RotationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=teamrotator;Username=postgres;Password=postgres");

        return new RotationDbContext(optionsBuilder.Options);
    }
} 