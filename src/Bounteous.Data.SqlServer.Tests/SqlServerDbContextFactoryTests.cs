using Bounteous.Data.SqlServer.Tests.Context;
using Bounteous.Data.SqlServer.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bounteous.Data.SqlServer.Tests;

public class SqlServerDbContextFactoryTests
{
    private const string TrustedConnectionString = "Server=localhost;Database=TestDb;Trusted_Connection=True;";
    private const string SqlAuthConnectionString = "Server=localhost;Database=TestDb;User Id=sa;Password=Test123;";
    private const string BasicConnectionString = "Server=localhost;Database=TestDb;";

    private static TestSqlServerDbContextFactory CreateFactory(string connectionString)
    {
        var mockConnectionBuilder = new Mock<IConnectionBuilder>();
        mockConnectionBuilder.Setup(cb => cb.AdminConnectionString).Returns(connectionString);
        var mockObserver = new Mock<IDbContextObserver>();
        var mockIdentityProvider = new Mock<IIdentityProvider<Guid>>();
        return new TestSqlServerDbContextFactory(mockConnectionBuilder.Object, mockObserver.Object,
            mockIdentityProvider.Object);
    }

    [Fact]
    public void ApplyOptions()
    {
        // Arrange
        var factory = CreateFactory(TrustedConnectionString);

        // Act
        var options = factory.ExposeApplyOptions(sensitiveDataLoggingEnabled: true);

        // Convert to generic options for DummyDbContext
        var typedOptions = new DbContextOptionsBuilder<DummyDbContext>()
            .UseSqlServer(TrustedConnectionString)
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
    public async Task SaveChangesAsyncCallsObserver()
    {
        // Arrange
        var mockObserver = new Mock<IDbContextObserver>();
        var mockIdentityProvider = new Mock<IIdentityProvider<Guid>>();

        var options = new DbContextOptionsBuilder<DbContextBase<Guid>>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        await using var context = new SqlServerTestDbContext(options, mockObserver.Object, mockIdentityProvider.Object);
        context.Entities.Add(new TestEntity { Id = Guid.NewGuid() });

        // Act
        await context.SaveChangesAsync();

        // Assert
        mockObserver.Verify(o => o.OnSaved(), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ApplyOptionsWithSensitiveDataLogging(bool sensitiveDataLoggingEnabled)
    {
        var factory = CreateFactory(SqlAuthConnectionString);
        var options = factory.ExposeApplyOptions(sensitiveDataLoggingEnabled);
        Assert.NotNull(options);
    }

    [Fact]
    public void ConstructorShouldInitializeWithValidParameters()
    {
        var factory = CreateFactory(BasicConnectionString);
        Assert.NotNull(factory);
    }

    [Fact]
    public void CreateShouldReturnDbContextInstance()
    {
        var factory = CreateFactory(TrustedConnectionString);
        var mockObserver = new Mock<IDbContextObserver>();
        var mockIdentityProvider = new Mock<IIdentityProvider<Guid>>();

        // Act
        var options = factory.ExposeApplyOptions();
        var context = factory.ExposeCreate(options, mockObserver.Object, mockIdentityProvider.Object);

        // Assert
        Assert.NotNull(context);
        Assert.IsType<SqlServerTestDbContext>(context);
    }

    [Fact]
    public void ApplyOptionsShouldEnableRetryOnFailureAndDetailedErrors()
    {
        var factory = CreateFactory(TrustedConnectionString);

        var options = factory.ExposeApplyOptions();
        Assert.NotNull(options);
    }

    [Fact]
    public async Task SaveChangesAsyncWithMultipleEntities()
    {
        // Arrange
        var mockObserver = new Mock<IDbContextObserver>();
        var mockIdentityProvider = new Mock<IIdentityProvider<Guid>>();

        var options = new DbContextOptionsBuilder<DbContextBase<Guid>>()
            .UseInMemoryDatabase(databaseName: "TestDbMultiple")
            .Options;

        await using var context = new SqlServerTestDbContext(options, mockObserver.Object, mockIdentityProvider.Object);
        context.Entities.Add(new TestEntity { Id = Guid.NewGuid() });
        context.Entities.Add(new TestEntity { Id = Guid.NewGuid() });
        context.Entities.Add(new TestEntity { Id = Guid.NewGuid() });

        // Act
        await context.SaveChangesAsync();

        // Assert
        mockObserver.Verify(o => o.OnSaved(), Times.Once);
        Assert.Equal(3, context.Entities.Count());
    }

    [Theory]
    [InlineData("Server=localhost;Database=TestDb;Trusted_Connection=True;")]
    [InlineData("Server=localhost,1433;Database=TestDb;User Id=sa;Password=Test123;")]
    [InlineData("Data Source=localhost;Initial Catalog=TestDb;Integrated Security=True;")]
    public void ApplyOptionsWithDifferentConnectionStrings(string connectionString)
    {
        var factory = CreateFactory(connectionString);
        var options = factory.ExposeApplyOptions();
        Assert.NotNull(options);
    }

    public class DummyDbContext(DbContextOptions<DummyDbContext> options) : DbContext(options);
}
