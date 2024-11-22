using Domain.Deployments;

namespace API.Infrastructure.Storage;

public class Memory : IDeploymentStorage
{
    public List<DeploymentDao> DeploymentDtos;

    public Memory()
    {
        DeploymentDtos = new List<DeploymentDao>();
    }

    public Task Store(DeploymentDao deploymentDao)
    {
        DeploymentDtos.Add(deploymentDao);

        return Task.CompletedTask;
    }
}