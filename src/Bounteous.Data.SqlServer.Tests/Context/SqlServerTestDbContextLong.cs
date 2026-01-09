using Bounteous.Data.SqlServer.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.SqlServer.Tests.Context;

public class SqlServerTestDbContextLong(
    DbContextOptions options,
    IDbContextObserver observer,
    IIdentityProvider<long> identityProvider)
    : DbContextBase<long>(options, observer, identityProvider)
{
    public DbSet<TestEntityLong> Entities => Set<TestEntityLong>();

    protected override void RegisterModels(ModelBuilder modelBuilder)
        => modelBuilder.Entity<TestEntityLong>().HasKey(e => e.Id);
}
