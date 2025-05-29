using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace TrinitySQL.Blueprint.Device;

[TestClass]
public class DeviceTypeScriptTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [TestCategory("Device")]
    public void RunTypeScriptTests()
    {
        var testEnvironment = new TypeScriptTestEnvironment(TestContext);
        testEnvironment.Validate();
        
        var testRunner = new NpmTestRunner(testEnvironment.WorkingDirectory);
        var result = testRunner.Execute();
        
        var reporter = new TestResultReporter(TestContext);
        reporter.WriteResults(result);
        
        if (result.HasFailures)
            Assert.Fail($"TypeScript tests failed with exit code {result.ExitCode}");
    }
}

public class TypeScriptTestEnvironment
{
    private readonly TestContext testContext;
    private readonly string workingDirectory;
    private readonly string npmCommand;

    public TypeScriptTestEnvironment(TestContext testContext)
    {
        this.testContext = testContext;
        this.workingDirectory = CalculateDeviceDirectory();
        this.npmCommand = DetermineNpmCommand();
    }

    public string WorkingDirectory => workingDirectory;

    public void Validate()
    {
        testContext.WriteLine($"Running TypeScript tests in: {workingDirectory}");
        
        ValidateDirectoryExists();
        ValidateNpmInstalled();
    }

    private void ValidateDirectoryExists()
    {
        if (!Directory.Exists(workingDirectory))
            Assert.Fail($"Device test directory not found: {workingDirectory}");
    }

    private void ValidateNpmInstalled()
    {
        try
        {
            var versionCheck = new ProcessBuilder()
                .WithCommand(npmCommand)
                .WithArguments("--version")
                .Build();

            using var process = Process.Start(versionCheck);
            process?.WaitForExit();
            
            if (process?.ExitCode != 0)
                Assert.Inconclusive("npm is not installed or not in PATH");
        }
        catch (Exception ex)
        {
            Assert.Inconclusive($"npm is not available: {ex.Message}");
        }
    }

    private static string CalculateDeviceDirectory()
    {
        var assemblyLocation = typeof(DeviceTypeScriptTests).Assembly.Location;
        var projectDir = Path.GetDirectoryName(assemblyLocation)!;
        return Path.GetFullPath(Path.Combine(projectDir, "..", "..", "..", "Device"));
    }

    private static string DetermineNpmCommand() =>
        OperatingSystem.IsWindows() ? "npm.cmd" : "npm";
}

public class NpmTestRunner
{
    private readonly string workingDirectory;
    private readonly string npmCommand;

    public NpmTestRunner(string workingDirectory)
    {
        this.workingDirectory = workingDirectory;
        this.npmCommand = OperatingSystem.IsWindows() ? "npm.cmd" : "npm";
    }

    public TestExecutionResult Execute()
    {
        var processInfo = new ProcessBuilder()
            .WithCommand(npmCommand)
            .WithArguments("test")
            .WithWorkingDirectory(workingDirectory)
            .WithOutputRedirection()
            .Build();

        using var process = new Process { StartInfo = processInfo };
        var outputCollector = new OutputCollector();
        
        process.OutputDataReceived += outputCollector.CollectOutput;
        process.ErrorDataReceived += outputCollector.CollectError;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        return new TestExecutionResult(
            process.ExitCode,
            outputCollector.Lines,
            outputCollector.HasFailureIndicators
        );
    }
}

public class ProcessBuilder
{
    private readonly ProcessStartInfo startInfo = new()
    {
        UseShellExecute = false,
        CreateNoWindow = true
    };

    public ProcessBuilder WithCommand(string command)
    {
        startInfo.FileName = command;
        return this;
    }

    public ProcessBuilder WithArguments(string arguments)
    {
        startInfo.Arguments = arguments;
        return this;
    }

    public ProcessBuilder WithWorkingDirectory(string directory)
    {
        startInfo.WorkingDirectory = directory;
        return this;
    }

    public ProcessBuilder WithOutputRedirection()
    {
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        return this;
    }

    public ProcessStartInfo Build() => startInfo;
}

public class OutputCollector
{
    private readonly List<string> lines = new();
    private bool hasFailureIndicators;

    public IReadOnlyList<string> Lines => lines;
    public bool HasFailureIndicators => hasFailureIndicators;

    public void CollectOutput(object sender, DataReceivedEventArgs e) =>
        CollectLine(e.Data);

    public void CollectError(object sender, DataReceivedEventArgs e) =>
        CollectLine(e.Data);

    private void CollectLine(string? line)
    {
        if (string.IsNullOrEmpty(line))
            return;
            
        lines.Add(line);
        
        if (line.Contains("FAIL"))
            hasFailureIndicators = true;
    }
}

public class TestExecutionResult
{
    public TestExecutionResult(int exitCode, IReadOnlyList<string> output, bool hasFailureIndicators)
    {
        ExitCode = exitCode;
        Output = output;
        HasFailures = hasFailureIndicators || exitCode != 0;
    }

    public int ExitCode { get; }
    public IReadOnlyList<string> Output { get; }
    public bool HasFailures { get; }
}

public class TestResultReporter
{
    private readonly TestContext testContext;

    public TestResultReporter(TestContext testContext)
    {
        this.testContext = testContext;
    }

    public void WriteResults(TestExecutionResult result)
    {
        testContext.WriteLine("\n=== TypeScript Test Results ===");
        
        foreach (var line in result.Output)
            testContext.WriteLine(line);
            
        testContext.WriteLine("===============================\n");
    }
}