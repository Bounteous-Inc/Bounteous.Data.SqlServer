using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.SqlServer.Tests.Context;

public class TestSqlServerDbContextFactory(
    IConnectionBuilder connectionBuilder,
    IDbContextObserver observer,
    IIdentityProvider<Guid> identityProvider)
    : SqlServerDbContextFactory<SqlServerTestDbContext, Guid>(connectionBuilder, observer, identityProvider)
{
    public DbContextOptions ExposeApplyOptions(bool sensitiveDataLoggingEnabled = false)
        => ApplyOptions(sensitiveDataLoggingEnabled);

    public SqlServerTestDbContext ExposeCreate(DbContextOptions options, IDbContextObserver observer,
        IIdentityProvider<Guid> identityProvider)
        => Create(options, observer, identityProvider);

    protected override SqlServerTestDbContext Create(DbContextOptions options, IDbContextObserver observer,
        IIdentityProvider<Guid> identityProvider) =>
        new(options, observer, identityProvider);
}