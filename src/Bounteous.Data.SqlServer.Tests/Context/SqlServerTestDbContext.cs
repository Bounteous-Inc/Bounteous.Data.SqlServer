using Bounteous.Data.SqlServer.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.SqlServer.Tests.Context;

public class SqlServerTestDbContext(
    DbContextOptions options,
    IDbContextObserver observer,
    IIdentityProvider<Guid> identityProvider)
    : DbContextBase<Guid>(options, observer, identityProvider)
{
    public DbSet<TestEntity> Entities => Set<TestEntity>();

    protected override void RegisterModels(ModelBuilder modelBuilder)
        => modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
}