using API.Infrastructure.Gateways;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;
using NSubstitute;

namespace API.Tests.Infrastructure.Gateways;

public class AzureGateway_GetDeployments
{
    [Fact]
    public async Task GetDeployments_ProjectHavingOneDeployment_DeploymentReturnWithCorrectMappings()
    {
        var connection = Substitute.For<IVssConnection>();

        var dateTimeFactory = Substitute.For<IDateTimeFactory>();

        dateTimeFactory.GetCurrentUTC().Returns(DateTime.Parse("2020-01-01 11:34"));

        var azureGateway = new AzureGateway(connection, dateTimeFactory);

        var releaseHttpClient2 = Substitute.For<ReleaseHttpClient2>(null, null);


        var referenceLinks = new ReferenceLinks();
        referenceLinks.AddLink("web", "WebLink");


        IPagedCollection<Deployment> deployments = new PageableCollection<Deployment>(new List<Deployment>
        {
            new()
            {
                Id = 1002, RequestedFor = new IdentityRef { UniqueName = "UserId" },
                ReleaseEnvironmentReference = new ReleaseEnvironmentShallowReference
                {
                    Name = "Stage"
                },
                CompletedOn = DateTime.Parse("2020-01-01 12:10"),
                Release = new ReleaseReference {  Links = referenceLinks }
            }
        }, "");

        var projectId = Guid.Parse("{596505FF-5AD0-42D7-856E-DBF62BB4415D}");


        releaseHttpClient2.GetDeploymentsAsync2(
            Guid.Parse("{596505FF-5AD0-42D7-856E-DBF62BB4415D}"),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<DateTime?>(),
            Arg.Any<DateTime?>(),
            DeploymentStatus.Succeeded,
            Arg.Any<DeploymentOperationStatus?>(),
            Arg.Any<bool?>(),
            Arg.Any<ReleaseQueryOrder?>(),
            100,
            0,
            Arg.Any<string>(),
            DateTime.Parse("2020-01-01 11:34").AddMonths(-12),
            Arg.Any<DateTime?>(),
            Arg.Any<string>(),
            Arg.Any<object>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(deployments));


        connection.GetClient<ReleaseHttpClient2>().Returns(releaseHttpClient2);

        var azureDeployments = await azureGateway.GetDeployments(projectId.ToString());

        Assert.Collection(azureDeployments, deployment =>
        {
            Assert.Equal("1002", deployment.Id);
            Assert.Equal("UserId", deployment.UserId);
            Assert.Equal("Stage", deployment.Stage);
            Assert.Equal(DateTime.Parse("2020-01-01 12:10"), deployment.Date);
            Assert.Equal("WebLink", deployment.WebLink);
        });
    }


    [Fact]
    public async Task GetDeployments_ProjectHavingTwoDeploymentOverPagedResult_PagedDeploymentReturn()
    {
        var connection = Substitute.For<IVssConnection>();

        var dateTimeFactory = Substitute.For<IDateTimeFactory>();

        dateTimeFactory.GetCurrentUTC().Returns(DateTime.Parse("2020-01-01 11:34"));

        var _azureGateway = new AzureGateway(connection, dateTimeFactory);

        var releaseHttpClient2 = Substitute.For<ReleaseHttpClient2>(null, null);


        IPagedCollection<Deployment> deployments = new PageableCollection<Deployment>(new List<Deployment>
        {
            new()
            {
                Id = 1, RequestedFor = new IdentityRef { UniqueName = "UserId" },
                ReleaseEnvironmentReference = new ReleaseEnvironmentShallowReference
                {
                    Name = "Stage"
                },
                CompletedOn = DateTime.Parse("2020-01-01 12:10")
            }
        }, "1");


        IPagedCollection<Deployment> deploymentsWithToken = new PageableCollection<Deployment>(new List<Deployment>
        {
            new()
            {
                Id = 2, RequestedFor = new IdentityRef { UniqueName = "UserId" },
                ReleaseEnvironmentReference = new ReleaseEnvironmentShallowReference
                {
                    Name = "Stage"
                },
                CompletedOn = DateTime.Parse("2020-01-01 12:10")
            }
        }, "");


        var projectId = Guid.Parse("{596505FF-5AD0-42D7-856E-DBF62BB4415D}");


        releaseHttpClient2.GetDeploymentsAsync2(
            Guid.Parse("{596505FF-5AD0-42D7-856E-DBF62BB4415D}"),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<DateTime?>(),
            Arg.Any<DateTime?>(),
            DeploymentStatus.Succeeded,
            Arg.Any<DeploymentOperationStatus?>(),
            Arg.Any<bool?>(),
            Arg.Any<ReleaseQueryOrder?>(),
            100,
            0,
            Arg.Any<string>(),
            DateTime.Parse("2020-01-01 11:34").AddMonths(-12),
            Arg.Any<DateTime?>(),
            Arg.Any<string>(),
            Arg.Any<object>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(deployments));

        releaseHttpClient2.GetDeploymentsAsync2(
            Guid.Parse("{596505FF-5AD0-42D7-856E-DBF62BB4415D}"),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<DateTime?>(),
            Arg.Any<DateTime?>(),
            DeploymentStatus.Succeeded,
            Arg.Any<DeploymentOperationStatus?>(),
            Arg.Any<bool?>(),
            Arg.Any<ReleaseQueryOrder?>(),
            100,
            1,
            Arg.Any<string>(),
            DateTime.Parse("2020-01-01 11:34").AddMonths(-12),
            Arg.Any<DateTime?>(),
            Arg.Any<string>(),
            Arg.Any<object>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult(deploymentsWithToken));


        connection.GetClient<ReleaseHttpClient2>().Returns(releaseHttpClient2);

        var azureDeployments = await _azureGateway.GetDeployments(projectId.ToString());


        Assert.Collection(azureDeployments, deployment => { Assert.Equal("1", deployment.Id); },
            deployment => { Assert.Equal("2", deployment.Id); });
    }
}