namespace TrinitySQL.Blueprint.Device.Interop;

public class TestResultReporter
{
    private readonly TestContext testContext;

    public TestResultReporter(TestContext testContext)
    {
        this.testContext = testContext;
    }

    public void ReportResults(TestExecutionResult result)
    {
        WriteHeader();
        WriteOutput(result.Output);
        WriteFooter();
    }

    public void ReportFailure(TestExecutionResult result)
    {
        Assert.Fail($"TypeScript tests failed with exit code {result.ExitCode}");
    }

    private void WriteHeader()
    {
        testContext.WriteLine("\n=== TypeScript Test Results ===");
    }

    private void WriteOutput(IReadOnlyList<string> output)
    {
        foreach (var line in output)
        {
            testContext.WriteLine(line);
        }
    }

    private void WriteFooter()
    {
        testContext.WriteLine("===============================\n");
    }
}