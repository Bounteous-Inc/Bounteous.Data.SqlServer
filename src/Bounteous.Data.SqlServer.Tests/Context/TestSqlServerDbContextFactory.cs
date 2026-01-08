using System;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.SqlServer.Tests.Context;

public class TestSqlServerDbContextFactory : SqlServerDbContextFactory<SqlServerTestDbContext, Guid>
{
    public TestSqlServerDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer)
        : base(connectionBuilder, observer) { }

    public DbContextOptions ExposeApplyOptions(bool sensitiveDataLoggingEnabled = false)
        => ApplyOptions(sensitiveDataLoggingEnabled);

    public SqlServerTestDbContext ExposeCreate(DbContextOptions options, IDbContextObserver observer)
        => Create(options, observer);

    protected override SqlServerTestDbContext Create(DbContextOptions options, IDbContextObserver observer) =>
        new(options, observer);
}