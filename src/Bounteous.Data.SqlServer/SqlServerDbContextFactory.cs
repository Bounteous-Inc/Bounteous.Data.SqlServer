using System;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.SqlServer;

public abstract class SqlServerDbContextFactory<T> : DbContextFactory<T> where T : IDbContext<Guid>
{
    public SqlServerDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer) 
        : base(connectionBuilder, observer)
    {
    }

    protected override DbContextOptions ApplyOptions(bool sensitiveDataLoggingEnabled = false)
        => new DbContextOptionsBuilder<DbContextBase<Guid>>()
            .UseSqlServer(ConnectionBuilder.AdminConnectionString,
                sqlOptions => { sqlOptions.EnableRetryOnFailure(); })
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled)
            .EnableDetailedErrors()
            .Options;
}