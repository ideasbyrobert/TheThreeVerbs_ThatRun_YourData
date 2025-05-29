namespace TrinitySQL.Blueprint.Device.Interop;

public interface IProcessExecutor
{
    string GetCommand();
    void ValidateEnvironment();
}