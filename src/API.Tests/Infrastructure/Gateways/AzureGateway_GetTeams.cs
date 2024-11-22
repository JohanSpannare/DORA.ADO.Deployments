using API.Infrastructure.Gateways;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using NSubstitute;
using NSubstitute.Extensions;

namespace API.Tests.Infrastructure.Gateways;

public class AzureGateway_GetTeams
{
    private readonly AzureGateway _azureGateway;
    private readonly TeamHttpClient? _projectHttpClient;


    public AzureGateway_GetTeams()
    {
        var connection = Substitute.For<IVssConnection>();

        _projectHttpClient = Substitute.For<TeamHttpClient>(null, null);

        connection.GetClient<TeamHttpClient>().Returns(_projectHttpClient);

        _azureGateway = new AzureGateway(connection, Substitute.For<IDateTimeFactory>());
    }

    [Fact]
    public async Task GetTeam_TeamsHavingMembers_TeamReturn()
    {
        var webApiTeams = new List<WebApiTeam>
        {
            new()
            {
                Id = Guid.Parse("{ABAE35A1-2EB6-4F80-B3B1-32E324A4335A}"), Name = "TeamName",
                ProjectId = Guid.Parse("40964261-a0ad-4f85-b301-1e35778c1055")
            }
        };

        _projectHttpClient
            .GetTeamsAsync(Arg.Is<string>("40964261-a0ad-4f85-b301-1e35778c1055"), Arg.Any<bool?>(), Arg.Is<int?>(100),
                Arg.Is<int?>(0), Arg.Any<bool?>(),
                Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(webApiTeams));

        _projectHttpClient.Configure()
            .GetTeamsAsync(Arg.Is<string>("40964261-a0ad-4f85-b301-1e35778c1055"), Arg.Any<bool?>(), Arg.Is<int?>(100),
                Arg.Is<int?>(1), Arg.Any<bool?>(),
                Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new List<WebApiTeam>()));


        var teamMembers = new List<TeamMember>
        {
            new() { Identity = new IdentityRef { UniqueName = "UniqueName1" } },
            new() { Identity = new IdentityRef { UniqueName = "UniqueName2" } }
        };


        _projectHttpClient
            .GetTeamMembersWithExtendedPropertiesAsync(Arg.Is<string>("40964261-a0ad-4f85-b301-1e35778c1055"),
                Arg.Is<string>("abae35a1-2eb6-4f80-b3b1-32e324a4335a"), Arg.Is<int?>(100),
                Arg.Is<int?>(0), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(teamMembers));


        _projectHttpClient.Configure()
            .GetTeamMembersWithExtendedPropertiesAsync(Arg.Is<string>("40964261-a0ad-4f85-b301-1e35778c1055"),
                Arg.Is<string>("abae35a1-2eb6-4f80-b3b1-32e324a4335a"), Arg.Is<int?>(100),
                Arg.Is<int?>(2), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<TeamMember>()));

        var teamDtos = await _azureGateway.GetTeams("40964261-a0ad-4f85-b301-1e35778c1055");


        Assert.Collection(teamDtos, dto =>
        {
            Assert.Equal("abae35a1-2eb6-4f80-b3b1-32e324a4335a", dto.Id);
            Assert.Equal("TeamName", dto.Name);

            Assert.Collection(dto.Members,
                userDto => { Assert.Equal("UniqueName1", userDto.Id); },
                userDto => { Assert.Equal("UniqueName2", userDto.Id); });
        });
    }

    [Fact]
    public async Task GetTeam_TeamsOverPagedResult_TeamsReturned()
    {
        var webApiTeamsFirst = new List<WebApiTeam>
        {
            new()
            {
                Id = Guid.Parse("{abae35a1-2eb6-4f80-b3b1-32e324a43351}"), Name = "TeamName1",
                ProjectId = Guid.Parse("{40964261-a0ad-4f85-b301-1e35778c1055}")
            }
        };

        _projectHttpClient
            .GetTeamsAsync(Arg.Is<string>("{40964261-a0ad-4f85-b301-1e35778c1055}"), Arg.Any<bool?>(),
                Arg.Is<int?>(100), Arg.Is<int?>(0), Arg.Any<bool?>(),
                Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(webApiTeamsFirst));


        var webApiTeamsSecond = new List<WebApiTeam>
        {
            new()
            {
                Id = Guid.Parse("{abae35a1-2eb6-4f80-b3b1-32e324a43352}"), Name = "TeamName2",
                ProjectId = Guid.Parse("{40964261-a0ad-4f85-b301-1e35778c1055}")
            }
        };

        _projectHttpClient.Configure()
            .GetTeamsAsync(Arg.Is<string>("{40964261-a0ad-4f85-b301-1e35778c1055}"), Arg.Any<bool?>(),
                Arg.Is<int?>(100), Arg.Is<int?>(1), Arg.Any<bool?>(),
                Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(webApiTeamsSecond));


        _projectHttpClient.Configure()
            .GetTeamsAsync(Arg.Is<string>("{40964261-a0ad-4f85-b301-1e35778c1055}"), Arg.Any<bool?>(),
                Arg.Is<int?>(100), Arg.Is<int?>(2), Arg.Any<bool?>(),
                Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new List<WebApiTeam>()));


        var team1MembersFirst = new List<TeamMember>
        {
            new() { Identity = new IdentityRef { UniqueName = "UniqueName1-1" } }
        };


        _projectHttpClient.Configure()
            .GetTeamMembersWithExtendedPropertiesAsync(Arg.Is<string>("40964261-a0ad-4f85-b301-1e35778c1055"),
                Arg.Is<string>("abae35a1-2eb6-4f80-b3b1-32e324a43351"), Arg.Is<int?>(100),
                Arg.Is<int?>(0), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(team1MembersFirst));

        var team1MembersSecond = new List<TeamMember>
        {
            new() { Identity = new IdentityRef { UniqueName = "UniqueName1-2" } }
        };


        _projectHttpClient.Configure()
            .GetTeamMembersWithExtendedPropertiesAsync(Arg.Is<string>("40964261-a0ad-4f85-b301-1e35778c1055"),
                Arg.Is<string>("abae35a1-2eb6-4f80-b3b1-32e324a43351"), Arg.Is<int?>(100),
                Arg.Is<int?>(1), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(team1MembersSecond));


        _projectHttpClient.Configure()
            .GetTeamMembersWithExtendedPropertiesAsync(Arg.Is<string>("40964261-a0ad-4f85-b301-1e35778c1055"),
                Arg.Is<string>("abae35a1-2eb6-4f80-b3b1-32e324a43351"), Arg.Is<int?>(100),
                Arg.Is<int?>(2), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<TeamMember>()));

        var team2MembersFirst = new List<TeamMember>
        {
            new() { Identity = new IdentityRef { UniqueName = "UniqueName2-1" } }
        };


        _projectHttpClient.Configure()
            .GetTeamMembersWithExtendedPropertiesAsync(Arg.Is<string>("40964261-a0ad-4f85-b301-1e35778c1055"),
                Arg.Is<string>("abae35a1-2eb6-4f80-b3b1-32e324a43352"), Arg.Is<int?>(100),
                Arg.Is<int?>(0), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(team2MembersFirst));

        var team2MembersSecond = new List<TeamMember>
        {
            new() { Identity = new IdentityRef { UniqueName = "UniqueName2-2" } }
        };


        _projectHttpClient.Configure()
            .GetTeamMembersWithExtendedPropertiesAsync(Arg.Is<string>("40964261-a0ad-4f85-b301-1e35778c1055"),
                Arg.Is<string>("abae35a1-2eb6-4f80-b3b1-32e324a43352"), Arg.Is<int?>(100),
                Arg.Is<int?>(1), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(team2MembersSecond));


        _projectHttpClient.Configure()
            .GetTeamMembersWithExtendedPropertiesAsync(Arg.Is<string>("40964261-a0ad-4f85-b301-1e35778c1055"),
                Arg.Is<string>("abae35a1-2eb6-4f80-b3b1-32e324a43352"), Arg.Is<int?>(100),
                Arg.Is<int?>(2), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<TeamMember>()));

        var teamDtos = await _azureGateway.GetTeams("{40964261-a0ad-4f85-b301-1e35778c1055}");


        Assert.Collection(teamDtos, dto =>
            {
                Assert.Equal("abae35a1-2eb6-4f80-b3b1-32e324a43351", dto.Id);
                Assert.Equal("TeamName1", dto.Name);

                Assert.Collection(dto.Members,
                    userDto => { Assert.Equal("UniqueName1-1", userDto.Id); },
                    userDto => { Assert.Equal("UniqueName1-2", userDto.Id); });
            },
            dto =>
            {
                Assert.Equal("abae35a1-2eb6-4f80-b3b1-32e324a43352", dto.Id);
                Assert.Equal("TeamName2", dto.Name);

                Assert.Collection(dto.Members,
                    userDto => { Assert.Equal("UniqueName2-1", userDto.Id); },
                    userDto => { Assert.Equal("UniqueName2-2", userDto.Id); });
            });
    }
}