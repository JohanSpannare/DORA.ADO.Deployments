namespace Domain.Deployments;

public interface IDeploymentStorage
{
    public Task Store(DeploymentDao deploymentDao);
}