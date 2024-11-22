using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Storage;

public class DeploymentContext : DbContext
{
    
    public DeploymentContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Deployment> Deployments { get; set; }

}