using Bounteous.Data.SqlServer.Tests.Context;
using Bounteous.Data.SqlServer.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bounteous.Data.SqlServer.Tests;

public class SqlServerDbContextFactoryTests
{
    [Fact]
    public void ApplyOptions_ShouldConfigureSqlServerOptionsCorrectly()
    {
        // Arrange
        var expectedConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=True;";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns(expectedConnectionString);

        var mockObserver = new Mock<IDbContextObserver>();
        var factory = new TestSqlServerDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);

        // Act
        var options = factory.ExposeApplyOptions(sensitiveDataLoggingEnabled: true);

        // Convert to generic options for DummyDbContext
        var typedOptions = new DbContextOptionsBuilder<DummyDbContext>()
            .UseSqlServer(expectedConnectionString)
            .Options;

        using var context = new DummyDbContext(typedOptions);
        var actualConnectionString = context.Database.GetDbConnection().ConnectionString;

        // Assert
        Assert.NotNull(options);
        Assert.Equal(expectedConnectionString, actualConnectionString);
    }


    [Fact]
    public async Task SaveChangesAsync_ShouldCallObserverOnSaved()
    {
        // Arrange
        var mockObserver = new Mock<IDbContextObserver>();

        var options = new DbContextOptionsBuilder<DbContextBase>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        await using var context = new SqlServerTestDbContext(options, mockObserver.Object);
        context.Entities.Add(new TestEntity { Id =Guid.NewGuid() });

        // Act
        await context.SaveChangesAsync();

        // Assert
        mockObserver.Verify(o => o.OnSaved(), Times.Once);
    }

    public class DummyDbContext : DbContext
    {
        public DummyDbContext(DbContextOptions<DummyDbContext> options)
            : base(options)
        {
        }
    }

}
