using Application.Deployments;
using Domain.Deployments;

namespace API.Deployments;

public class DeploymentBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _factory;

    public DeploymentBackgroundService(IServiceScopeFactory factory)
    {
        _factory = factory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _factory.CreateScope();

        var deploymentServiceSettings = scope.ServiceProvider.GetRequiredService<DeploymentServiceSettings>();
        var deploymentService = scope.ServiceProvider.GetRequiredService<DeploymentService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DeploymentBackgroundService>>();

        try
        {
            do
            {
                var timeSpan = TimeSpan.Parse(deploymentServiceSettings.TimeSpan);
                await deploymentService.Process();
                await Task.Delay(timeSpan, stoppingToken);
            } while (stoppingToken.IsCancellationRequested == false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unhandled error occured");
        }
    }
}