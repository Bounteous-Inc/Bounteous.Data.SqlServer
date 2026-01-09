using System;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.SqlServer;

public abstract class SqlServerDbContextFactory<T, TUserId>(
    IConnectionBuilder connectionBuilder,
    IDbContextObserver observer,
    IIdentityProvider<TUserId> identityProvider)
    : DbContextFactory<T, TUserId>(connectionBuilder, observer, identityProvider)
    where T : IDbContext<TUserId>
    where TUserId : struct
{
    protected override DbContextOptions ApplyOptions(bool sensitiveDataLoggingEnabled = false)
        => new DbContextOptionsBuilder<DbContextBase<TUserId>>()
            .UseSqlServer(ConnectionBuilder.AdminConnectionString,
                sqlOptions => { sqlOptions.EnableRetryOnFailure(); })
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled)
            .EnableDetailedErrors()
            .Options;
}