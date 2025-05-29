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
        var outputCollector = new TestOutputCollector();
        var process = CreateTestProcess();
        
        return ExecuteProcess(process, outputCollector);
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
}