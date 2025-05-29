namespace TrinitySQL.Blueprint.Device.Interop;

public class TestEnvironmentValidator
{
    private readonly string workingDirectory;
    private readonly IProcessExecutor processExecutor;

    public TestEnvironmentValidator(string workingDirectory, IProcessExecutor processExecutor)
    {
        this.workingDirectory = workingDirectory;
        this.processExecutor = processExecutor;
    }

    public void Validate(TestContext testContext)
    {
        testContext.WriteLine($"Running TypeScript tests in: {workingDirectory}");
        
        ValidateDirectoryExists();
        processExecutor.ValidateEnvironment();
    }

    private void ValidateDirectoryExists()
    {
        if (!Directory.Exists(workingDirectory))
        {
            Assert.Fail($"Device test directory not found: {workingDirectory}");
        }
    }
}