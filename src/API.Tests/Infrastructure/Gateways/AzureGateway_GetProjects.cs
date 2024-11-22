using API.Infrastructure.Gateways;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using NSubstitute;

namespace API.Tests.Infrastructure.Gateways;

public class AzureGateway_GetProjects
{
    private readonly AzureGateway _azureGateway;

    public AzureGateway_GetProjects()
    {
        var connection = Substitute.For<IVssConnection>();

        var projectReferences = (IPagedList<TeamProjectReference>)new PagedList<TeamProjectReference>
        {
            new() { Id = Guid.Parse("{35DC7516-4109-4C01-8149-DD931F2B6D11}") },
            new() { Id = Guid.Parse("{35DC7516-4109-4C01-8149-DD931F2B6D12}") }
        };

        var projectHttpClient = Substitute.For<ProjectHttpClient>(null, null);

        projectHttpClient
            .GetProjects(Arg.Any<ProjectState?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<object>(),
                Arg.Any<string>(),
                Arg.Any<bool?>()).Returns(Task.FromResult(projectReferences));

        projectHttpClient
            .GetProject("35dc7516-4109-4c01-8149-dd931f2b6d11").Returns(Task.FromResult(new TeamProject(){DefaultTeam = new WebApiTeam(){Name = "DefaultTeam"}}));
        projectHttpClient
            .GetProject("35dc7516-4109-4c01-8149-dd931f2b6d12").Returns(Task.FromResult(new TeamProject() { DefaultTeam = new WebApiTeam() { Name = "DefaultTeam" } }));

        connection.GetClient<ProjectHttpClient>().Returns(projectHttpClient);


        _azureGateway = new AzureGateway(connection, Substitute.For<IDateTimeFactory>());
    }

    [Fact]
    public async Task GetProjects_TwoProjectsExists_Successful()
    {
        var azureProjects = await _azureGateway.GetProjects();

        Assert.Collection(azureProjects,
            project => { Assert.Equal("35DC7516-4109-4C01-8149-DD931F2B6D11".ToLowerInvariant(), project.Id); },
            project => { Assert.Equal("35DC7516-4109-4C01-8149-DD931F2B6D12".ToLowerInvariant(), project.Id); });
    }
}