using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.SqlServer.Tests.Context;

public class TestSqlServerDbContextFactory : SqlServerDbContextFactory<SqlServerTestDbContext>
{
    public TestSqlServerDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer)
        : base(connectionBuilder, observer) { }

    public DbContextOptions<DbContextBase> ExposeApplyOptions(bool sensitiveDataLoggingEnabled = false)
        => ApplyOptions(sensitiveDataLoggingEnabled);

    protected override SqlServerTestDbContext Create(DbContextOptions<DbContextBase> options, IDbContextObserver observer) =>
        new(options, observer);
}