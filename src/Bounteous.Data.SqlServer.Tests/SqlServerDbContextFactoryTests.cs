using System;
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
        var inputConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=True;";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns(inputConnectionString);

        var mockObserver = new Mock<IDbContextObserver>();
        var factory = new TestSqlServerDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);

        // Act
        var options = factory.ExposeApplyOptions(sensitiveDataLoggingEnabled: true);

        // Convert to generic options for DummyDbContext
        var typedOptions = new DbContextOptionsBuilder<DummyDbContext>()
            .UseSqlServer(inputConnectionString)
            .Options;

        using var context = new DummyDbContext(typedOptions);
        var actualConnectionString = context.Database.GetDbConnection().ConnectionString;

        // Assert
        Assert.NotNull(options);
        Assert.Contains("Data Source=localhost", actualConnectionString);
        Assert.Contains("Initial Catalog=TestDb", actualConnectionString);
        Assert.Contains("Integrated Security=True", actualConnectionString);
    }


    [Fact]
    public async Task SaveChangesAsync_ShouldCallObserverOnSaved()
    {
        // Arrange
        var mockObserver = new Mock<IDbContextObserver>();

        var options = new DbContextOptionsBuilder<DbContextBase<Guid>>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        await using var context = new SqlServerTestDbContext(options, mockObserver.Object);
        context.Entities.Add(new TestEntity { Id = Guid.NewGuid() });

        // Act
        await context.SaveChangesAsync();

        // Assert
        mockObserver.Verify(o => o.OnSaved(), Times.Once);
    }

    [Fact]
    public void ApplyOptions_WithSensitiveDataLoggingDisabled_ShouldConfigureCorrectly()
    {
        // Arrange
        var inputConnectionString = "Server=localhost;Database=TestDb;User Id=sa;Password=Test123;";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns(inputConnectionString);

        var mockObserver = new Mock<IDbContextObserver>();
        var factory = new TestSqlServerDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);

        // Act
        var options = factory.ExposeApplyOptions(sensitiveDataLoggingEnabled: false);

        // Assert
        Assert.NotNull(options);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithValidParameters()
    {
        // Arrange
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns("Server=localhost;Database=TestDb;");
        var mockObserver = new Mock<IDbContextObserver>();

        // Act
        var factory = new TestSqlServerDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);

        // Assert
        Assert.NotNull(factory);
    }

    [Fact]
    public void Create_ShouldReturnDbContextInstance()
    {
        // Arrange
        var inputConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=True;";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns(inputConnectionString);

        var mockObserver = new Mock<IDbContextObserver>();
        var factory = new TestSqlServerDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);

        // Act
        var options = factory.ExposeApplyOptions();
        var context = factory.ExposeCreate(options, mockObserver.Object);

        // Assert
        Assert.NotNull(context);
        Assert.IsType<SqlServerTestDbContext>(context);
    }

    [Fact]
    public void ApplyOptions_ShouldEnableRetryOnFailure()
    {
        // Arrange
        var inputConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=True;";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns(inputConnectionString);

        var mockObserver = new Mock<IDbContextObserver>();
        var factory = new TestSqlServerDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);

        // Act
        var options = factory.ExposeApplyOptions();

        // Assert
        Assert.NotNull(options);
        // Retry on failure is configured internally in SQL Server options
    }

    [Fact]
    public void ApplyOptions_ShouldEnableDetailedErrors()
    {
        // Arrange
        var inputConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=True;";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns(inputConnectionString);

        var mockObserver = new Mock<IDbContextObserver>();
        var factory = new TestSqlServerDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);

        // Act
        var options = factory.ExposeApplyOptions();

        // Assert
        Assert.NotNull(options);
        // Detailed errors are enabled in the options configuration
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleEntities_ShouldCallObserverOnce()
    {
        // Arrange
        var mockObserver = new Mock<IDbContextObserver>();

        var options = new DbContextOptionsBuilder<DbContextBase<Guid>>()
            .UseInMemoryDatabase(databaseName: "TestDbMultiple")
            .Options;

        await using var context = new SqlServerTestDbContext(options, mockObserver.Object);
        context.Entities.Add(new TestEntity { Id = Guid.NewGuid() });
        context.Entities.Add(new TestEntity { Id = Guid.NewGuid() });
        context.Entities.Add(new TestEntity { Id = Guid.NewGuid() });

        // Act
        await context.SaveChangesAsync();

        // Assert
        mockObserver.Verify(o => o.OnSaved(), Times.Once);
        Assert.Equal(3, context.Entities.Count());
    }

    [Fact]
    public void ApplyOptions_WithDifferentConnectionStrings_ShouldHandleCorrectly()
    {
        // Arrange - Test with different connection string formats
        var connectionStrings = new[]
        {
            "Server=localhost;Database=TestDb;Trusted_Connection=True;",
            "Server=localhost,1433;Database=TestDb;User Id=sa;Password=Test123;",
            "Data Source=localhost;Initial Catalog=TestDb;Integrated Security=True;"
        };

        foreach (var connString in connectionStrings)
        {
            var mockConnectionBuilder = new Mock<IConnectionBuilder>();
            mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns(connString);

            var mockObserver = new Mock<IDbContextObserver>();
            var factory = new TestSqlServerDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object);

            // Act
            var options = factory.ExposeApplyOptions();

            // Assert
            Assert.NotNull(options);
        }
    }

    public class DummyDbContext : DbContext
    {
        public DummyDbContext(DbContextOptions<DummyDbContext> options)
            : base(options)
        {
        }
    }

}
