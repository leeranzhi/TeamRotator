using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TeamRotator.Infrastructure.Data;

namespace TeamRotator.Tests;

public abstract class TestBase
{
    protected readonly Mock<ILogger<T>> CreateLogger<T>() where T : class
    {
        return new Mock<ILogger<T>>();
    }

    protected RotationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RotationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new RotationDbContext(options);
    }

    protected IDbContextFactory<RotationDbContext> CreateDbContextFactory()
    {
        var options = new DbContextOptionsBuilder<RotationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var factoryMock = new Mock<IDbContextFactory<RotationDbContext>>();
        factoryMock.Setup(f => f.CreateDbContext())
            .Returns(() => new RotationDbContext(options));

        return factoryMock.Object;
    }
} 