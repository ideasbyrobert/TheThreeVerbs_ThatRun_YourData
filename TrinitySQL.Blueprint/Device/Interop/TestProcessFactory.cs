using System.Diagnostics;

namespace TrinitySQL.Blueprint.Device.Interop;

public class TestProcessFactory
{
    private readonly string executableName;

    public TestProcessFactory(string executableName)
    {
        this.executableName = executableName;
    }

    public Process CreateProcess(string arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executableName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        return new Process { StartInfo = startInfo };
    }
}