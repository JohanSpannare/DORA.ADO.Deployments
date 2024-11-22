using API.Deployments;
using API.Infrastructure.Gateways;
using API.Infrastructure.Tasks;
using Domain.Deployments;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
using Microsoft.VisualStudio.Services.WebApi;
using NSubstitute;
using Deployment = Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Deployment;

namespace API.Tests.Deployments;

public class DeploymentBackgroundService_Execute : IClassFixture<MockedAPIFactory<Program>>
{
    private readonly ManualResetEvent _manualResetEvent;
    private IList<DeploymentDao> _storedDeployments;

    public DeploymentBackgroundService_Execute(MockedAPIFactory<Program> factory)
    {
        IPagedCollection<Deployment> deployments = new PageableCollection<Deployment>(new List<Deployment>
        {
            new()
            {
                Id = 1002, RequestedFor = new IdentityRef { UniqueName = "UserId" },
                ReleaseEnvironmentReference = new ReleaseEnvironmentShallowReference
                {
                    Name = "Prod"
                },
                CompletedOn = DateTime.Parse("2020-01-01 12:10")
            }
        }, "");


        var azureProjects = (IPagedList<TeamProjectReference>)new PagedList<TeamProjectReference>
        {
            new() { Id = Guid.Parse("{35DC7516-4109-4C01-8149-DD931F2B6D11}") }
        };


        var webApiTeams = new List<WebApiTeam> { new() { Id = Guid.Parse("{9ABE7E32-DE8B-4C0F-B810-1F54EE3110E2}") } };
        var teamMembers = new List<TeamMember> { new() { Identity = new IdentityRef { UniqueName = "UniqueName" } } };

        _manualResetEvent = new ManualResetEvent(false);

        _ = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var vssConnection = Substitute.For<IVssConnection>();
                services.AddTransient<IVssConnection>(_ => vssConnection);


                var deploymentStorage = Substitute.For<IDeploymentStorage>();
                _storedDeployments = new List<DeploymentDao>();

                services.AddSingleton<IDeploymentStorage>(_ => deploymentStorage);

                services.AddSingleton<DeploymentServiceSettings>(_ => new DeploymentServiceSettings
                    { TimeSpan = "00:00:01" });


                var taskRunner = Substitute.For<ITaskRunner>();
                taskRunner.Run(Arg.Any<Func<Task>>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask).AndDoes(
                    info =>
                    {
                        var task = info.Arg<Func<Task>>();
                        task.Invoke();
                        _manualResetEvent.Set();
                    });

                services.AddTransient<ITaskRunner>(provider => taskRunner);

                deploymentStorage.When(x => x.Store(Arg.Any<DeploymentDao>()))
                    .Do(x => { _storedDeployments.Add(x.Arg<DeploymentDao>()); });

                var projectHttpClient = Substitute.For<ProjectHttpClient>(null, null);
                vssConnection.GetClient<ProjectHttpClient>().Returns(projectHttpClient);

                projectHttpClient
                    .GetProjects(Arg.Any<ProjectState?>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<object>(),
                        Arg.Any<string>(),
                        Arg.Any<bool?>()).Returns(Task.FromResult(azureProjects));

                projectHttpClient
                    .GetProject(default).ReturnsForAnyArgs(Task.FromResult(new TeamProject(){DefaultTeam = new WebApiTeam(){Name = "DefaultTeam"}}));


                var teamHttpClient = Substitute.For<TeamHttpClient>(null, null);
                vssConnection.GetClient<TeamHttpClient>().Returns(teamHttpClient);

                teamHttpClient.GetTeamsAsync(default)
                    .ReturnsForAnyArgs(Task.FromResult(webApiTeams), Task.FromResult(new List<WebApiTeam>()));


                teamHttpClient
                    .GetTeamMembersWithExtendedPropertiesAsync(default, default)
                    .ReturnsForAnyArgs(Task.FromResult(teamMembers), Task.FromResult(new List<TeamMember>()));


                var releaseHttpClient2 = Substitute.For<ReleaseHttpClient2>(null, null);
                vssConnection.GetClient<ReleaseHttpClient2>().Returns(releaseHttpClient2);

                releaseHttpClient2.GetDeploymentsAsync2(
                    Arg.Any<Guid>(),
                    Arg.Any<int?>(),
                    Arg.Any<int?>(),
                    Arg.Any<string>(),
                    Arg.Any<DateTime?>(),
                    Arg.Any<DateTime?>(),
                    Arg.Any<DeploymentStatus?>(),
                    Arg.Any<DeploymentOperationStatus?>(),
                    Arg.Any<bool?>(),
                    Arg.Any<ReleaseQueryOrder?>(),
                    100,
                    0,
                    Arg.Any<string>(),
                    Arg.Any<DateTime?>(),
                    Arg.Any<DateTime?>(),
                    Arg.Any<string>(),
                    Arg.Any<object>(),
                    Arg.Any<CancellationToken>()
                ).Returns(Task.FromResult(deployments));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task Execute_ProcessOneDeployment_Successful()
    {
        _manualResetEvent.WaitOne(TimeSpan.FromSeconds(5));

        Assert.True(_storedDeployments.Count >= 2);

        var dao = _storedDeployments.First();

        Assert.Equal("1002", dao.Id);
        Assert.Equal("DefaultTeam", dao.TeamName);
        Assert.Equal("PROD", dao.Environment);
        Assert.Equal(DateTime.Parse("2020-01-01 12:10"), dao.Date);
    }
}