using Bounteous.Data.SqlServer.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.SqlServer.Tests.Context;

public class SqlServerTestDbContext : DbContextBase
{
    public DbSet<TestEntity> Entities => Set<TestEntity>();

    public SqlServerTestDbContext(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
        : base(options, observer) { }

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
    }
}