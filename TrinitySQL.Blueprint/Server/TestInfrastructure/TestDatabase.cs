using TrinitySQL.Server.Data;
using TrinitySQL.Server.Core.SharedKernel;

namespace TrinitySQL.Blueprint.Server.TestInfrastructure;

public class TestDatabase : IDisposable
{
    private readonly string _dbPath;

    public TestDatabase(TheaterSalesContext context, IQueryDispatcher queryDispatcher, string dbPath)
    {
        Context = context;
        QueryDispatcher = queryDispatcher;
        _dbPath = dbPath;
    }

    public TheaterSalesContext Context { get; }
    public IQueryDispatcher QueryDispatcher { get; }

    public void Dispose()
    {
        Context?.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}