using TrinitySQL.Server.Data;
using TrinitySQL.Server.Core.SharedKernel;

namespace TrinitySQL.Blueprint.Server.TestInfrastructure;

public abstract class QueryHandlerTestBase
{
    protected TheaterSalesContext Context { get; private set; } = null!;
    protected IQueryDispatcher Dispatcher { get; private set; } = null!;
    private TestDatabase? _testDatabase;

    [TestInitialize]
    public void BaseInitialize()
    {
        _testDatabase = new TestDatabaseBuilder().Build();
        Context = _testDatabase.Context;
        Dispatcher = _testDatabase.QueryDispatcher;
        InitializeTest();
    }

    [TestCleanup]
    public void BaseCleanup()
    {
        CleanupTest();
        _testDatabase?.Dispose();
    }

    protected virtual void InitializeTest() { }
    protected virtual void CleanupTest() { }

    protected List<TResult> ExecuteQuery<TQuery, TResult>(TQuery query) 
        where TQuery : IQuery<IEnumerable<TResult>>
    {
        return Dispatcher
            .Dispatch<TQuery, IEnumerable<TResult>>(query)
            .ToList();
    }
}