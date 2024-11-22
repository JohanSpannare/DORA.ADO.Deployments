namespace API.Infrastructure.Tasks;

public class TaskRunner : ITaskRunner
{
    public Task Run(Func<Task> func, CancellationToken token)
    {
        return Task.Run(func, token);
    }
}