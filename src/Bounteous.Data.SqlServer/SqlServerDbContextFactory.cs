using System;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.SqlServer;

public abstract class SqlServerDbContextFactory<T, TUserId> : DbContextFactory<T, TUserId> 
    where T : IDbContext<TUserId> 
    where TUserId : struct
{
    protected SqlServerDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer) 
        : base(connectionBuilder, observer)
    {
    }

    protected override DbContextOptions ApplyOptions(bool sensitiveDataLoggingEnabled = false)
        => new DbContextOptionsBuilder<DbContextBase<TUserId>>()
            .UseSqlServer(ConnectionBuilder.AdminConnectionString,
                sqlOptions => { sqlOptions.EnableRetryOnFailure(); })
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled)
            .EnableDetailedErrors()
            .Options;
}