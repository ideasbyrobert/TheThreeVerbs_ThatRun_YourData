using TrinitySQL.Blueprint.Device.Interop;

namespace TrinitySQL.Blueprint.Device.Interop;

[TestClass]
public class DeviceTypeScriptTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [TestCategory("Device")]
    public void RunTypeScriptTests()
    {
        var workingDirectory = CalculateDeviceDirectory();
        var processExecutor = new NpmProcessExecutor();
        
        var validator = new TestEnvironmentValidator(workingDirectory, processExecutor);
        validator.Validate(TestContext);
        
        var runner = new TypeScriptTestRunner(workingDirectory);
        var result = runner.Execute();
        
        var reporter = new TestResultReporter(TestContext);
        reporter.ReportResults(result);
        
        if (result.HasFailures)
        {
            reporter.ReportFailure(result);
        }
    }

    private static string CalculateDeviceDirectory()
    {
        var assemblyLocation = typeof(DeviceTypeScriptTests).Assembly.Location;
        var binDir = Path.GetDirectoryName(assemblyLocation)!;
        var projectRoot = Path.GetFullPath(Path.Combine(binDir, "..", "..", ".."));
        return Path.Combine(projectRoot, "Device");
    }
}