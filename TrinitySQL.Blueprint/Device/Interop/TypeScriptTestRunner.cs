using System.Diagnostics;

namespace TrinitySQL.Blueprint.Device.Interop;

public class TypeScriptTestRunner
{
    private readonly string workingDirectory;
    private readonly IProcessExecutor processExecutor;

    public TypeScriptTestRunner(string workingDirectory)
    {
        this.workingDirectory = workingDirectory;
        this.processExecutor = new NpmProcessExecutor();
    }

    public TestExecutionResult Execute()
    {
        EnsureDependenciesInstalled();
        
        var outputCollector = new TestOutputCollector();
        var process = CreateTestProcess();
        
        return ExecuteProcess(process, outputCollector);
    }
    
    private void EnsureDependenciesInstalled()
    {
        var nodeModulesPath = Path.Combine(workingDirectory, "node_modules");
        if (!Directory.Exists(nodeModulesPath))
        {
            InstallDependencies();
        }
        
        EnsureDeviceDependenciesInstalled();
    }
    
    private void InstallDependencies()
    {
        var processFactory = new TestProcessFactory(processExecutor.GetCommand());
        var installProcess = processFactory.CreateProcess("install", workingDirectory);
        
        installProcess.Start();
        installProcess.WaitForExit();
        
        if (installProcess.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to install npm dependencies. Exit code: {installProcess.ExitCode}");
        }
    }

    private Process CreateTestProcess()
    {
        var processFactory = new TestProcessFactory(processExecutor.GetCommand());
        return processFactory.CreateProcess("test", workingDirectory);
    }

    private TestExecutionResult ExecuteProcess(Process process, TestOutputCollector outputCollector)
    {
        process.OutputDataReceived += outputCollector.CollectOutput;
        process.ErrorDataReceived += outputCollector.CollectError;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        return new TestExecutionResult(
            process.ExitCode,
            outputCollector.GetOutput(),
            outputCollector.HasFailures()
        );
    }
    
    private void EnsureDeviceDependenciesInstalled()
    {
        var devicePath = Path.GetFullPath(Path.Combine(workingDirectory, "..", "..", "TrinitySQL.Device"));
        var deviceNodeModulesPath = Path.Combine(devicePath, "node_modules");
        
        if (!Directory.Exists(deviceNodeModulesPath))
        {
            InstallDeviceDependencies(devicePath);
        }
    }
    
    private void InstallDeviceDependencies(string devicePath)
    {
        var processFactory = new TestProcessFactory(processExecutor.GetCommand());
        var installProcess = processFactory.CreateProcess("install", devicePath);
        
        installProcess.Start();
        installProcess.WaitForExit();
        
        if (installProcess.ExitCode != 0)
        {
            throw new InvalidOperationException($"Failed to install TrinitySQL.Device dependencies. Exit code: {installProcess.ExitCode}");
        }
    }
}