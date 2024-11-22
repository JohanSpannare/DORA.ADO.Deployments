using API.Infrastructure.Storage;
using Domain.Deployments;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Infrastructure.Storage;

public class MSSQL_Store
{
    private readonly MSSql _msSql;
    private readonly DeploymentContext _deploymentContext;

    public MSSQL_Store()
    {
        var builder = new DbContextOptionsBuilder<DeploymentContext>();
        builder.UseInMemoryDatabase(databaseName: "DeploymentDbInMemory");

        var dbContextOptions = builder.Options;
        _deploymentContext = new DeploymentContext(dbContextOptions);


        // Delete existing db before creating a new one
        _deploymentContext.Database.EnsureDeleted();
        _deploymentContext.Database.EnsureCreated();


        _msSql = new MSSql(_deploymentContext);
    }

    [Fact]
    public async Task Store_ValidDeployment_Stored()
    {
        await _msSql.Store(new DeploymentDao(){Id = "1",TeamName = "TeamName",Environment = "Test",Date = DateTime.Parse("2020-01-01 11:00")});

        var deployments = _deploymentContext.Deployments.Where(x => x.id == "1").ToList();

        Assert.Collection(deployments, deployment =>
        {
            Assert.Equal("1",deployment.id);
            Assert.Equal("TeamName",deployment.teamname);
            Assert.Equal("Test",deployment.environment);
            Assert.Equal(DateTime.Parse("2020-01-01 11:00"), deployment.deploymenttime);
        });
    }
}