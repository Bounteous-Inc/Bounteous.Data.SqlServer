using System;
using Bounteous.Data.SqlServer.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bounteous.Data.SqlServer.Tests.Context;

public class SqlServerTestDbContext : DbContextBase<Guid>
{
    public DbSet<TestEntity> Entities => Set<TestEntity>();

    public SqlServerTestDbContext(DbContextOptions options, IDbContextObserver observer)
        : base(options, observer) { }

    protected override void RegisterModels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
    }
}