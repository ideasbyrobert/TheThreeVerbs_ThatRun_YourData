using System.Diagnostics;

namespace TrinitySQL.Blueprint.Device.Interop;

public class NpmProcessExecutor : IProcessExecutor
{
    public string GetCommand()
    {
        return OperatingSystem.IsWindows() ? "npm.cmd" : "npm";
    }

    public void ValidateEnvironment()
    {
        try
        {
            var versionCheck = Process.Start(new ProcessStartInfo
            {
                FileName = GetCommand(),
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            
            versionCheck?.WaitForExit();
            
            if (versionCheck?.ExitCode != 0)
            {
                Assert.Inconclusive("npm is not installed or not in PATH");
            }
        }
        catch (Exception ex)
        {
            Assert.Inconclusive($"npm is not available: {ex.Message}");
        }
    }
}