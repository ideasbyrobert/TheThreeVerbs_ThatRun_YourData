namespace TrinitySQL.Server.Core.SharedKernel;

public interface IQuery<TResult>
{
    string QueryName => GetType().Name;
}