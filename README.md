# Bounteous.Data.SqlServer

A specialized Entity Framework Core data access library for SQL Server databases in .NET 8+ applications. This library extends the base `Bounteous.Data` functionality with SQL Server-specific configurations, optimizations, and database provider settings for enterprise-grade applications.

## 📦 Installation

Install the package via NuGet:

```bash
dotnet add package Bounteous.Data.SqlServer
```

Or via Package Manager Console:

```powershell
Install-Package Bounteous.Data.SqlServer
```

## 🚀 Quick Start

### 1. Configure Services

```csharp
using Bounteous.Data.SqlServer;
using Microsoft.Extensions.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    // Register the module
    services.AddModule<ModuleStartup>();
    
    // Register your connection string provider
    services.AddSingleton<IConnectionStringProvider, MyConnectionStringProvider>();
    
    // Register your SQL Server DbContext factory
    services.AddScoped<IDbContextFactory<MyDbContext>, MyDbContextFactory>();
}
```

### 2. Create Your SQL Server DbContext Factory

```csharp
using Bounteous.Data.SqlServer;
using Microsoft.EntityFrameworkCore;

public class MyDbContextFactory : SqlServerDbContextFactory<MyDbContext>
{
    public MyDbContextFactory(IConnectionBuilder connectionBuilder, IDbContextObserver observer)
        : base(connectionBuilder, observer)
    {
    }

    protected override MyDbContext Create(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
    {
        return new MyDbContext(options, observer);
    }
}
```

### 3. Configure Connection String Provider

```csharp
using Bounteous.Data;
using Microsoft.Extensions.Configuration;

public class MyConnectionStringProvider : IConnectionStringProvider
{
    private readonly IConfiguration _configuration;

    public MyConnectionStringProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string ConnectionString => _configuration.GetConnectionString("SqlServerConnection") 
        ?? throw new InvalidOperationException("SQL Server connection string not found");
}
```

### 4. Use Your SQL Server Context

```csharp
public class CustomerService
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;

    public CustomerService(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Customer> CreateCustomerAsync(string name, string email, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserId(userId);
        
        var customer = new Customer 
        { 
            Name = name, 
            Email = email 
        };
        
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        
        return customer;
    }
}
```

## 🏗️ Architecture Overview

Bounteous.Data.SqlServer builds upon the foundation of `Bounteous.Data` and provides SQL Server-specific enhancements:

- **SQL Server Provider Integration**: Uses `Microsoft.EntityFrameworkCore.SqlServer` for optimal SQL Server performance
- **Connection Resilience**: Built-in retry policies for SQL Server connection failures
- **SQL Server-Specific Optimizations**: Configured for SQL Server's unique characteristics and features
- **Enterprise Features**: Support for SQL Server enterprise features like Always On, clustering, and advanced security
- **Audit Trail Support**: Inherits automatic auditing from base `Bounteous.Data`
- **Soft Delete Support**: Logical deletion capabilities optimized for SQL Server
- **Performance Optimization**: Leverages SQL Server's advanced query optimization and indexing capabilities

## 🔧 Key Features

### SQL Server-Specific DbContext Factory

The `SqlServerDbContextFactory<T>` class provides SQL Server-optimized configuration:

```csharp
public abstract class SqlServerDbContextFactory<T> : DbContextFactory<T> where T : IDbContext
{
    protected override DbContextOptions<DbContextBase> ApplyOptions(bool sensitiveDataLoggingEnabled = false)
    {
        return new DbContextOptionsBuilder<DbContextBase>()
            .UseSqlServer(ConnectionBuilder.AdminConnectionString, sqlOptions => 
            { 
                sqlOptions.EnableRetryOnFailure(); 
            })
            .EnableSensitiveDataLogging(sensitiveDataLoggingEnabled)
            .EnableDetailedErrors()
            .Options;
    }
}
```

**Features:**
- **Retry on Failure**: Automatic retry for transient SQL Server connection issues
- **Sensitive Data Logging**: Configurable logging for debugging (disabled in production)
- **Detailed Errors**: Enhanced error reporting for development
- **SQL Server Provider**: Uses official Microsoft SQL Server Entity Framework provider
- **Connection Pooling**: Leverages SQL Server's built-in connection pooling
- **Transaction Support**: Full support for SQL Server transactions and isolation levels

### Connection Management

SQL Server-specific connection handling with built-in resilience:

```csharp
// Connection string format for SQL Server
"Server=localhost;Database=MyDatabase;Integrated Security=true;"

// With additional SQL Server-specific options
"Server=localhost;Database=MyDatabase;Integrated Security=true;" +
"Connection Timeout=30;" +
"Command Timeout=30;" +
"Pooling=true;" +
"Min Pool Size=0;" +
"Max Pool Size=100;" +
"MultipleActiveResultSets=true;"
```

### SQL Server Enterprise Features

The library supports SQL Server enterprise features:

```csharp
// Always On Availability Groups
"Server=AGListener;Database=MyDatabase;Integrated Security=true;" +
"ApplicationIntent=ReadOnly;" +
"MultiSubnetFailover=true;"

// Azure SQL Database
"Server=tcp:myserver.database.windows.net,1433;Database=MyDatabase;" +
"User Id=username;Password=password;" +
"Encrypt=true;" +
"TrustServerCertificate=false;" +
"Connection Timeout=30;"
```

## 📚 Usage Examples

### Basic CRUD Operations with SQL Server

```csharp
public class ProductService
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;

    public ProductService(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Product> CreateProductAsync(string name, decimal price, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserId(userId);
        
        var product = new Product 
        { 
            Name = name, 
            Price = price,
            CreatedOn = DateTime.UtcNow
        };
        
        context.Products.Add(product);
        await context.SaveChangesAsync();
        
        return product;
    }

    public async Task<List<Product>> GetProductsAsync(int page = 1, int size = 50)
    {
        using var context = _contextFactory.Create();
        
        return await context.Products
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedOn)
            .ToPaginatedListAsync(page, size);
    }

    public async Task<Product> UpdateProductAsync(Guid productId, string name, decimal price, Guid userId)
    {
        using var context = _contextFactory.Create().WithUserId(userId);
        
        var product = await context.Products.FindById(productId);
        product.Name = name;
        product.Price = price;
        
        await context.SaveChangesAsync();
        return product;
    }
}
```

### SQL Server-Specific Query Operations

```csharp
public class OrderService
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;

    public OrderService(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var context = _contextFactory.Create();
        
        return await context.Orders
            .Where(o => o.CreatedOn >= startDate && o.CreatedOn <= endDate)
            .Where(o => !o.IsDeleted)
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedOn)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalSalesAsync(DateTime startDate, DateTime endDate)
    {
        using var context = _contextFactory.Create();
        
        return await context.Orders
            .Where(o => o.CreatedOn >= startDate && o.CreatedOn <= endDate)
            .Where(o => !o.IsDeleted)
            .SumAsync(o => o.TotalAmount);
    }

    // SQL Server-specific window functions
    public async Task<List<CustomerSales>> GetTopCustomersBySalesAsync(int topCount)
    {
        using var context = _contextFactory.Create();
        
        return await context.Orders
            .Where(o => !o.IsDeleted)
            .GroupBy(o => o.CustomerId)
            .Select(g => new CustomerSales
            {
                CustomerId = g.Key,
                TotalSales = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderByDescending(cs => cs.TotalSales)
            .Take(topCount)
            .ToListAsync();
    }
}
```

### Soft Delete Operations

```csharp
public async Task DeleteProductAsync(Guid productId, Guid userId)
{
    using var context = _contextFactory.Create().WithUserId(userId);
    
    var product = await context.Products.FindById(productId);
    
    // Soft delete - sets IsDeleted = true
    product.IsDeleted = true;
    
    await context.SaveChangesAsync();
}
```

### SQL Server-Specific Features

```csharp
public class ReportService
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;

    public ReportService(IDbContextFactory<MyDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // Using SQL Server's JSON functions
    public async Task<List<Order>> GetOrdersWithMetadataAsync(string metadataKey)
    {
        using var context = _contextFactory.Create();
        
        return await context.Orders
            .Where(o => EF.Functions.JsonValue(o.Metadata, $"$.{metadataKey}") != null)
            .Where(o => !o.IsDeleted)
            .ToListAsync();
    }

    // Using SQL Server's full-text search
    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        using var context = _contextFactory.Create();
        
        return await context.Products
            .Where(p => EF.Functions.Contains(p.Name, searchTerm) || 
                       EF.Functions.Contains(p.Description, searchTerm))
            .Where(p => !p.IsDeleted)
            .ToListAsync();
    }

    // Using SQL Server's temporal tables (if configured)
    public async Task<List<Product>> GetProductHistoryAsync(Guid productId, DateTime asOfDate)
    {
        using var context = _contextFactory.Create();
        
        return await context.Products
            .TemporalAsOf(asOfDate)
            .Where(p => p.Id == productId)
            .ToListAsync();
    }
}
```

## 🔧 Configuration Options

### SQL Server Connection String Options

```csharp
// Basic connection string
"Server=localhost;Database=MyDatabase;Integrated Security=true;"

// With additional SQL Server options
"Server=localhost;Database=MyDatabase;Integrated Security=true;" +
"Connection Timeout=30;" +
"Command Timeout=30;" +
"Pooling=true;" +
"Min Pool Size=0;" +
"Max Pool Size=100;" +
"MultipleActiveResultSets=true;" +
"TrustServerCertificate=false;"
```

### SQL Server-Specific DbContext Configuration

```csharp
public class MyDbContext : DbContextBase
{
    public MyDbContext(DbContextOptions<DbContextBase> options, IDbContextObserver observer)
        : base(options, observer)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure SQL Server-specific settings
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
            
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");
            
        // Configure JSON columns
        modelBuilder.Entity<Order>()
            .Property(o => o.Metadata)
            .HasColumnType("nvarchar(max)");
            
        // Configure indexes for performance
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Name)
            .HasDatabaseName("IX_Products_Name");
            
        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.CustomerId, o.CreatedOn })
            .HasDatabaseName("IX_Orders_CustomerId_CreatedOn");
    }
}
```

### Advanced SQL Server Features

```csharp
// Configure temporal tables
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    modelBuilder.Entity<Product>()
        .ToTable("Products", t => t.IsTemporal());
        
    modelBuilder.Entity<Order>()
        .ToTable("Orders", t => t.IsTemporal());
}

// Configure computed columns
modelBuilder.Entity<Product>()
    .Property(p => p.FullDescription)
    .HasComputedColumnSql("[Name] + ' - ' + [Description]");

// Configure check constraints
modelBuilder.Entity<Product>()
    .HasCheckConstraint("CK_Products_Price", "[Price] > 0");

// Configure sequences
modelBuilder.HasSequence<int>("OrderNumberSequence")
    .StartsAt(1000)
    .IncrementsBy(1);
    
modelBuilder.Entity<Order>()
    .Property(o => o.OrderNumber)
    .HasDefaultValueSql("NEXT VALUE FOR OrderNumberSequence");
```

## 🎯 Target Framework

- **.NET 8.0** and later

## 📋 Dependencies

- **Bounteous.Core** (0.0.16) - Core utilities and patterns
- **Bounteous.Data** (0.0.15) - Base data access functionality
- **Microsoft.EntityFrameworkCore** (9.0.9) - Entity Framework Core
- **Microsoft.EntityFrameworkCore.SqlServer** (9.0.9) - SQL Server provider for EF Core
- **EntityFrameworkCore.NamingConventions** (8.0.0) - Naming convention support
- **Microsoft.Extensions.Configuration.Abstractions** (9.0.9) - Configuration management

## 🔗 Related Projects

- [Bounteous.Data](../Bounteous.Data/) - Base data access library
- [Bounteous.Core](../Bounteous.Core/) - Core utilities and patterns
- [Bounteous.Data.MySQL](../Bounteous.Data.MySQL/) - MySQL-specific implementation
- [Bounteous.Data.PostgreSQL](../Bounteous.Data.PostgreSQL/) - PostgreSQL-specific implementation

## 🤝 Contributing

This library is maintained by Xerris Inc. For contributions, please contact the development team.

## 📄 License

See [LICENSE](LICENSE) file for details.

---

*This library provides SQL Server-specific enhancements to the Bounteous.Data framework, ensuring optimal performance and compatibility with SQL Server databases including enterprise features in .NET applications.*
