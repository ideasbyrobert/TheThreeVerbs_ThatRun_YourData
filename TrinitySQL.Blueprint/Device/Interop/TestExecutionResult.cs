namespace TrinitySQL.Blueprint.Device.Interop;

public class TestExecutionResult
{
    public TestExecutionResult(int exitCode, IReadOnlyList<string> output, bool hasFailures)
    {
        ExitCode = exitCode;
        Output = output;
        HasFailures = hasFailures || exitCode != 0;
    }

    public int ExitCode { get; }
    public IReadOnlyList<string> Output { get; }
    public bool HasFailures { get; }
}