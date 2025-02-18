using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.SqlServer;

public abstract class SqlServerDbContextFactory<T> : DbContextFactory<T> where T : DbContext
{
    public SqlServerDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer) 
        : base(connectionBuilder, observer)
    {
    }

    protected override DbContextOptions<DbContextBase> ApplyOptions(bool sensitiveDataLoggingEnabled = false)
        => new DbContextOptionsBuilder<DbContextBase>()
            .UseSqlServer(ConnectionBuilder.AdminConnectionString,
                sqlOptions => { sqlOptions.EnableRetryOnFailure(); })
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled: sensitiveDataLoggingEnabled)
            .EnableDetailedErrors()
            .Options;
}