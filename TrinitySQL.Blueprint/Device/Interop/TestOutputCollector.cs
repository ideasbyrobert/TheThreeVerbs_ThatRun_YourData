using System.Diagnostics;

namespace TrinitySQL.Blueprint.Device.Interop;

public class TestOutputCollector
{
    private readonly List<string> output = new();
    private bool hasFailureIndicators;

    public void CollectOutput(object sender, DataReceivedEventArgs e)
    {
        CollectLine(e.Data);
    }

    public void CollectError(object sender, DataReceivedEventArgs e)
    {
        CollectLine(e.Data);
    }

    public IReadOnlyList<string> GetOutput() => output;

    public bool HasFailures() => hasFailureIndicators;

    private void CollectLine(string? line)
    {
        if (string.IsNullOrEmpty(line))
            return;

        output.Add(line);
        
        if (line.Contains("FAIL"))
            hasFailureIndicators = true;
    }
}