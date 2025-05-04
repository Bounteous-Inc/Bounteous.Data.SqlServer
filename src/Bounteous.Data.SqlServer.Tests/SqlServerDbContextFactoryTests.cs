using Bounteous.Data.SqlServer.Tests.Context;
using Bounteous.Data.SqlServer.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

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

        // Assert
        var extension = options.Extensions.OfType<SqlServerOptionsExtension>().FirstOrDefault();
        Assert.NotNull(extension);
        Assert.Equal(expectedConnectionString, extension.ConnectionString);
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

}
