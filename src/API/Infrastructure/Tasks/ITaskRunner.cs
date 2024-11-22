namespace API.Infrastructure.Tasks;

public interface ITaskRunner
{
    Task Run(Func<Task> func, CancellationToken token);
}