using Microsoft.EntityFrameworkCore;
using TrinitySQL.Server.Data;
using TrinitySQL.Server.Infrastructure;

namespace TrinitySQL.Blueprint.Server.TestInfrastructure;

public class TestDatabaseBuilder
{
    public TestDatabase Build()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"theater_sales_test_{Guid.NewGuid()}.db");
        var connectionString = $"Data Source={dbPath}";

        var initializer = new DatabaseInitializer(connectionString);
        initializer.Initialize();
        initializer.SeedData();

        var optionsBuilder = new DbContextOptionsBuilder<TheaterSalesContext>();
        optionsBuilder.UseSqlite(connectionString);
        
        var context = new TheaterSalesContext(optionsBuilder.Options);
        var serviceConfig = new ServiceConfiguration(context);
        
        return new TestDatabase(context, serviceConfig.QueryDispatcher, dbPath);
    }
}