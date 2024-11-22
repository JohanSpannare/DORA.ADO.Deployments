using Domain.Deployments;

namespace API.Infrastructure.Storage;

public class MSSql : IDeploymentStorage
{
    private readonly DeploymentContext _context;

    public MSSql(DeploymentContext context)
    {
        _context = context;

        _context.Database.EnsureCreated();
    }

    public Task Store(DeploymentDao deploymentDao)
    {
        var deployment = new Deployment(deploymentDao.Id, deploymentDao.Environment, deploymentDao.TeamName, deploymentDao.Date);

        var deployments = _context.Deployments.Where(x => x.id == deployment.id);

        if (deployments.Any())
        {
            _context.RemoveRange(deployments);
        }

        _context.Add(deployment);

        _context.SaveChanges();

        return Task.CompletedTask;

    }
}