using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.SqlServer.Tests.Context;

public class TestSqlServerDbContextFactoryLong(
    IConnectionBuilder connectionBuilder,
    IDbContextObserver observer,
    IIdentityProvider<long> identityProvider)
    : SqlServerDbContextFactory<SqlServerTestDbContextLong, long>(connectionBuilder, observer, identityProvider)
{
    public DbContextOptions ExposeApplyOptions(bool sensitiveDataLoggingEnabled = false)
        => ApplyOptions(sensitiveDataLoggingEnabled);

    public SqlServerTestDbContextLong ExposeCreate(DbContextOptions options, IDbContextObserver observer,
        IIdentityProvider<long> identityProvider)
        => Create(options, observer, identityProvider);

    protected override SqlServerTestDbContextLong Create(DbContextOptions options, IDbContextObserver observer,
        IIdentityProvider<long> identityProvider) =>
        new(options, observer, identityProvider);
}
