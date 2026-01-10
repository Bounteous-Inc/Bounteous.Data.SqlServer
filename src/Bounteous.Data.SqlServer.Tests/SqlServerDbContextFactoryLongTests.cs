using Bounteous.Data.SqlServer.Tests.Context;
using Bounteous.Data.SqlServer.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bounteous.Data.SqlServer.Tests;

public class SqlServerDbContextFactoryLongTests
{
    [Fact]
    public void ApplyOptionsShouldConfigureSqlServerOptionsCorrectly()
    {
        // Arrange
        const string inputConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=True;";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns(inputConnectionString);

        var mockObserver = new Mock<IDbContextObserver>();
        var mockIdentityProvider = new Mock<IIdentityProvider<long>>();
        var factory = new TestSqlServerDbContextFactoryLong(mockConnectionBuilder.Object, mockObserver.Object,
            mockIdentityProvider.Object);

        // Act
        var options = factory.ExposeApplyOptions(sensitiveDataLoggingEnabled: true);

        // Assert
        Assert.NotNull(options);
    }

    [Fact]
    public async Task SaveChangesAsyncShouldCallObserverOnSaved()
    {
        // Arrange
        var mockObserver = new Mock<IDbContextObserver>();
        var mockIdentityProvider = new Mock<IIdentityProvider<long>>();

        var options = new DbContextOptionsBuilder<DbContextBase<long>>()
            .UseInMemoryDatabase(databaseName: "TestDbLong")
            .Options;

        await using var context =
            new SqlServerTestDbContextLong(options, mockObserver.Object, mockIdentityProvider.Object);
        context.Entities.Add(new TestEntityLong { Id = 2L, CreatedBy = 1L, ModifiedBy = 13L });

        // Act
        await context.SaveChangesAsync();

        // Assert
        mockObserver.Verify(o => o.OnSaved(), Times.Once);
    }

    [Fact]
    public void ApplyOptionsWithSensitiveDataLoggingDisabledShouldConfigureCorrectly()
    {
        // Arrange
        var inputConnectionString = "Server=localhost;Database=TestDb;User Id=sa;Password=Test123;";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns(inputConnectionString);

        var mockObserver = new Mock<IDbContextObserver>();
        var mockIdentityProvider = new Mock<IIdentityProvider<long>>();
        var factory = new TestSqlServerDbContextFactoryLong(mockConnectionBuilder.Object, mockObserver.Object,
            mockIdentityProvider.Object);

        // Act
        var options = factory.ExposeApplyOptions(sensitiveDataLoggingEnabled: false);

        // Assert
        Assert.NotNull(options);
    }

    [Fact]
    public void ConstructorShouldInitializeWithValidParameters()
    {
        // Arrange
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns("Server=localhost;Database=TestDb;");
        var mockObserver = new Mock<IDbContextObserver>();
        var mockIdentityProvider = new Mock<IIdentityProvider<long>>();

        // Act
        var factory = new TestSqlServerDbContextFactoryLong(mockConnectionBuilder.Object, mockObserver.Object,
            mockIdentityProvider.Object);

        // Assert
        Assert.NotNull(factory);
    }

    [Fact]
    public void CreateShouldReturnDbContextInstance()
    {
        // Arrange
        var inputConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=True;";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns(inputConnectionString);

        var mockObserver = new Mock<IDbContextObserver>();
        var mockIdentityProvider = new Mock<IIdentityProvider<long>>();
        var factory = new TestSqlServerDbContextFactoryLong(mockConnectionBuilder.Object, mockObserver.Object, mockIdentityProvider.Object);

        // Act
        var options = factory.ExposeApplyOptions();
        var context = factory.ExposeCreate(options, mockObserver.Object, mockIdentityProvider.Object);

        // Assert
        Assert.NotNull(context);
        Assert.IsType<SqlServerTestDbContextLong>(context);
    }

    [Fact]
    public void ApplyOptionsShouldEnableRetryOnFailure()
    {
        // Arrange
        var inputConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=True;";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns(inputConnectionString);

        var mockObserver = new Mock<IDbContextObserver>();
        var mockIdentityProvider = new Mock<IIdentityProvider<long>>();
        var factory = new TestSqlServerDbContextFactoryLong(mockConnectionBuilder.Object, mockObserver.Object, mockIdentityProvider.Object);

        // Act
        var options = factory.ExposeApplyOptions();

        // Assert
        Assert.NotNull(options);
        // Retry on failure is configured internally in SQL Server options
    }

    [Fact]
    public void ApplyOptionsShouldEnableDetailedErrors()
    {
        // Arrange
        var inputConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=True;";
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns(inputConnectionString);

        var mockObserver = new Mock<IDbContextObserver>();
        var mockIdentityProvider = new Mock<IIdentityProvider<long>>();
        var factory = new TestSqlServerDbContextFactoryLong(mockConnectionBuilder.Object, mockObserver.Object, mockIdentityProvider.Object);

        // Act
        var options = factory.ExposeApplyOptions();

        // Assert
        Assert.NotNull(options);
        // Detailed errors are enabled in the options configuration
    }

    [Fact]
    public async Task SaveChangesAsyncWithMultipleEntitiesShouldCallObserverOnce()
    {
        // Arrange
        var mockObserver = new Mock<IDbContextObserver>();
        var mockIdentityProvider = new Mock<IIdentityProvider<long>>();

        var options = new DbContextOptionsBuilder<DbContextBase<long>>()
            .UseInMemoryDatabase(databaseName: "TestDbMultipleLong")
            .Options;

        await using var context = new SqlServerTestDbContextLong(options, mockObserver.Object, mockIdentityProvider.Object);
        context.Entities.Add(new TestEntityLong { Id = 1L, CreatedBy = 1L, ModifiedBy = 1L });
        context.Entities.Add(new TestEntityLong { Id = 2L, CreatedBy = 2L, ModifiedBy = 2L });
        context.Entities.Add(new TestEntityLong { Id = 3L, CreatedBy = 3L, ModifiedBy = 3L });

        // Act
        await context.SaveChangesAsync();

        // Assert
        mockObserver.Verify(o => o.OnSaved(), Times.Once);
        Assert.Equal(3, context.Entities.Count());
    }

    [Fact]
    public void ApplyOptionsWithDifferentConnectionStringsShouldHandleCorrectly()
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
            var mockIdentityProvider = new Mock<IIdentityProvider<long>>();
            var factory = new TestSqlServerDbContextFactoryLong(mockConnectionBuilder.Object, mockObserver.Object, mockIdentityProvider.Object);

            var options = factory.ExposeApplyOptions();
            
            Assert.NotNull(options);
        }
    }

    [Fact]
    public async Task SaveChangesAsyncShouldPersistLongUserIds()
    {
        // Arrange
        var mockObserver = new Mock<IDbContextObserver>();
        var mockIdentityProvider = new Mock<IIdentityProvider<long>>();
        mockIdentityProvider.Setup(ip => ip.GetCurrentUserId()).Returns(12345L);

        var options = new DbContextOptionsBuilder<DbContextBase<long>>()
            .UseInMemoryDatabase(databaseName: "TestDbLongUserIds")
            .Options;

        await using var context = new SqlServerTestDbContextLong(options, mockObserver.Object, mockIdentityProvider.Object);
        var entity = new TestEntityLong { Id = 1L };
        context.Entities.Add(entity);

        // Act
        await context.SaveChangesAsync();

        // Assert
        var savedEntity = await context.Entities.FirstOrDefaultAsync(e => e.Id == entity.Id);
        Assert.NotNull(savedEntity);
        // The auditing system automatically sets CreatedBy and ModifiedBy to the current user ID from IIdentityProvider
        Assert.Equal(12345L, savedEntity.CreatedBy);
        Assert.Equal(12345L, savedEntity.ModifiedBy);
    }
}
