using Domain.Deployments;

namespace Application.Deployments;

internal class DeploymentRepository
{
    private readonly IDeploymentStorage _deploymentstorage;

    public DeploymentRepository(IDeploymentStorage deploymentstorage)
    {
        _deploymentstorage = deploymentstorage;
    }

    internal void Save(Deployment deployment)
    {
        _deploymentstorage.Store(new DeploymentDao
            { Id = deployment.Id, Environment = deployment.Stage, TeamName = deployment.Team, Date = deployment.Date });
    }
}