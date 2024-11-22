using Application.Deployments;
using Domain;
using Domain.Deployments;
using Domain.Projects;
using Domain.Teams;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Application.Tests.Services;

public class DeploymentService_Process
{
    private IGetDeployments _getDeployments;
    private IGetProjects _getProjects;
    private IGetTeams _getTeams;

    private readonly DeploymentService _deploymentService;
    private readonly ILogger<DeploymentService> _logger;
    private readonly List<DeploymentDao> _storedDeployments;

    public DeploymentService_Process()
    {
        _getDeployments = Substitute.For<IGetDeployments>();
        _getProjects = Substitute.For<IGetProjects>();
        _getTeams = Substitute.For<IGetTeams>();
        var deploymentStorage = Substitute.For<IDeploymentStorage>();

        _storedDeployments = new List<DeploymentDao>();

        _logger = Substitute.For<ILogger<DeploymentService>>();

        var mappings = new Dictionary<string, string>();

        mappings.Add("757f7da4-a58d-4662-98f4-8b324f210f96", "DEV");
        mappings.Add("41bc097f-9258-4aa2-aae1-1a8601b3d987", "DEV");
        mappings.Add("f4fa8c93-2eb3-4583-b72b-06b662b5d0d8", "TEST");
        mappings.Add("7d63c0c5-d9c1-4cfa-94c5-c8349a2346aa", "TEST");
        mappings.Add("53e9d5e7-5a7d-44d8-b78b-2404a0b49731", "TEST");
        mappings.Add("b35e0e73-8c66-4e18-965c-0d27ccddb1c2", "PROD");
        mappings.Add("cae1836e-1a46-424b-a5b4-ab01ab0ca49d", "PROD");
        mappings.Add("e8ffa795-a2d1-41fe-bc43-a2e8c85ca8b4", "PROD");
        mappings.Add("5095dee9-6420-44ed-a707-d541dad60503", "PROD");
        mappings.Add("42a332ef-c48f-45e4-9e9d-ac0e384fff9e", "PROD");
        mappings.Add("b5c30330-52c2-46b9-b037-18b5246b027f", "");
        mappings.Add("f085d7e9-c832-4cc3-af52-91cccf06671e", "");
        mappings.Add("environment", "environment");

        var validEnvironments = new ValidEnvironments(mappings);

        _deploymentService = new DeploymentService(_getDeployments, _getProjects, _getTeams, deploymentStorage, _logger, validEnvironments);

        deploymentStorage.When(x => x.Store(Arg.Any<DeploymentDao>()))
            .Do(x => { _storedDeployments.Add(x.Arg<DeploymentDao>()); });
    }

    [Fact]
    public async Task Process_DeploymentsHavingTeam_Success()
    {
        //Arrange
        _getProjects.GetProjects().Returns(new List<ProjectDto>
        {
            new() { Id = "Project1",DefaultTeam = "DefaultTeam"}
        });

        _getTeams.GetTeams("Project1").Returns(new List<TeamDto>
        {
            new()
            {
                Id = "Team1", Name = "TeamName1",
                Members = new List<UserDto> { new() { Id = "UserId1" } }
            }
        });

        _getDeployments.GetDeployments("Project1").Returns(new List<AzureDeployment>
        {
            new()
            {
                Id = "deploymentId1", Stage = "environment", UserId = "UserId1", Date = DateTime.Parse("2020-01-01")
            }
        });

        //Act
        await _deploymentService.Process();


        //Assert
        Assert.Collection(_storedDeployments, firstDeployment =>
        {
            Assert.Equal("deploymentId1", firstDeployment.Id);
            Assert.Equal("environment", firstDeployment.Environment);
            Assert.Equal("TeamName1", firstDeployment.TeamName);
            Assert.Equal(DateTime.Parse("2020-01-01"), firstDeployment.Date);
        });

        _logger.Assert(LogLevel.Information, "Processing project [Id: {projectId}]");
        _logger.Assert(LogLevel.Information, "Processing project [Id: {\"Id\":\"Project1\",\"DefaultTeamName\":\"DefaultTeam\"}]");
    }

    [Fact]
    public async Task Process_DeploymentsWithNoTeam_UseUserIdInTeamName()
    {
        //Arrange
        _getProjects.GetProjects().Returns(new List<ProjectDto>
        {
            new() { Id = "Project1",DefaultTeam = "DefaultTeam"}
        });

        _getTeams.GetTeams("Project1").Returns(new List<TeamDto>());

        _getDeployments.GetDeployments("Project1").Returns(new List<AzureDeployment>
        {
            new()
            {
                Id = "deploymentId1", Stage = "environment", UserId = "UserId1", Date = DateTime.Parse("2020-01-01")
            }
        });


        //Act
        await _deploymentService.Process();


        //Assert
        Assert.Collection(_storedDeployments, firstDeployment =>
        {
            Assert.Equal("deploymentId1", firstDeployment.Id);
            Assert.Equal("environment", firstDeployment.Environment);
            Assert.Equal("DefaultTeam", firstDeployment.TeamName);
            Assert.Equal(DateTime.Parse("2020-01-01"), firstDeployment.Date);
        });
    }

    [Fact]
    public async Task Process_DeploymentsHavingMultipleTeams_UseUserIdInTeamNameAndCreateWarning()
    {
        //Arrange
        _getProjects.GetProjects().Returns(new List<ProjectDto>
        {
            new() { Id = "Project1",DefaultTeam = "DefaultTeam" }
        });

        _getTeams.GetTeams("Project1").Returns(new List<TeamDto>
        {
            new()
            {
                Id = "Team1", Name = "TeamName1",
                Members = new List<UserDto> { new() { Id = "UserId1" } }
            },
            new()
            {
                Id = "Team2", Name = "TeamName2",
                Members = new List<UserDto> { new() { Id = "UserId1" } }
            }
        });

        _getDeployments.GetDeployments("Project1").Returns(new List<AzureDeployment>
        {
            new()
            {
                Id = "deploymentId1", Stage = "environment", UserId = "UserId1", Date = DateTime.Parse("2020-01-01")
            }
        });

        //Act
        await _deploymentService.Process();


        //Assert
        Assert.Collection(_storedDeployments, firstDeployment =>
        {
            Assert.Equal("deploymentId1", firstDeployment.Id);
            Assert.Equal("environment", firstDeployment.Environment);
            Assert.Equal("DefaultTeam", firstDeployment.TeamName);
            Assert.Equal(DateTime.Parse("2020-01-01"), firstDeployment.Date);
        });

        _logger.Assert(LogLevel.Warning, "User is member of multiple teams [User: {user}, Teams: {teams}]");
        _logger.Assert(LogLevel.Warning,
            "User is member of multiple teams [User: UserId1, Teams: TeamName1|TeamName2]");
    }

    [Fact]
    public async Task Process_DeploymentUserNotMemberOfTeam_UseUserIdInTeamName()
    {
        //Arrange
        _getProjects.GetProjects().Returns(new List<ProjectDto>
        {
            new() { Id = "Project1",DefaultTeam = "DefaultTeam" }
        });

        _getTeams.GetTeams("Project1").Returns(new List<TeamDto>
        {
            new()
            {
                Id = "Team1", Name = "TeamName1",
                Members = new List<UserDto> { new() { Id = "UserIdX" } }
            }
        });

        _getDeployments.GetDeployments("Project1").Returns(new List<AzureDeployment>
        {
            new()
            {
                Id = "deploymentId1", Stage = "environment", UserId = "UserId1", Date = DateTime.Parse("2020-01-01")
            }
        });


        //Act
        await _deploymentService.Process();


        //Assert
        Assert.Collection(_storedDeployments, firstDeployment =>
        {
            Assert.Equal("deploymentId1", firstDeployment.Id);
            Assert.Equal("environment", firstDeployment.Environment);
            Assert.Equal("DefaultTeam", firstDeployment.TeamName);
            Assert.Equal(DateTime.Parse("2020-01-01"), firstDeployment.Date);
        });

        _logger.Assert(LogLevel.Warning, "User is not a member of any teams [User: {user}]");
        _logger.Assert(LogLevel.Warning, "User is not a member of any teams [User: UserId1]");
    }

    [Theory]
    //[InlineData("Sign Off")]
    //[InlineData("signed Off")]
    //[InlineData("Approver sign off")]
    //[InlineData("Team sign off")]
    //[InlineData("PO sign off for PRD release")]
    //[InlineData("PO Signoff")]
    //[InlineData("PO sign off")]
    //[InlineData("PO sign off for PRD")]
    [InlineData("b5c30330-52c2-46b9-b037-18b5246b027e", null)]
    [InlineData("757f7da4-a58d-4662-98f4-8b324f210f96", "DEV")]
    [InlineData("41bc097f-9258-4aa2-aae1-1a8601b3d987", "DEV")]
    [InlineData("f4fa8c93-2eb3-4583-b72b-06b662b5d0d8", "TEST")]
    [InlineData("7d63c0c5-d9c1-4cfa-94c5-c8349a2346aa", "TEST")]
    [InlineData("53e9d5e7-5a7d-44d8-b78b-2404a0b49731", "TEST")]
    [InlineData("b35e0e73-8c66-4e18-965c-0d27ccddb1c2", "PROD")]
    [InlineData("cae1836e-1a46-424b-a5b4-ab01ab0ca49d", "PROD")]
    [InlineData("e8ffa795-a2d1-41fe-bc43-a2e8c85ca8b4", "PROD")]
    [InlineData("5095dee9-6420-44ed-a707-d541dad60503", "PROD")]
    [InlineData("42a332ef-c48f-45e4-9e9d-ac0e384fff9e", "PROD")]
    [InlineData("f085d7e9-c832-4cc3-af52-91cccf06671g", null)]
    [InlineData("f085d7e9-c832-4cc3-af52-91cccf06671e", "")]
    public async Task Process_DeploymentThatShouldBeExcluded_NotMapped(string rawEnvironment, string environment)
    {
        //Arrange
        _getProjects.GetProjects().Returns(new List<ProjectDto>
        {
            new() { Id = "Project1",DefaultTeam = "DefaultTeam" }
        });

        _getTeams.GetTeams("Project1").Returns(new List<TeamDto>
        {
            new()
            {
                Id = "Team1", Name = "TeamName1",
                Members = new List<UserDto> { new() { Id = "UserId1" } }
            }
        });

        _getDeployments.GetDeployments("Project1").Returns(new List<AzureDeployment>
        {
            new()
            {
                Id = "deploymentId1", Stage = rawEnvironment.ToUpper(), UserId = "UserId1",
                Date = DateTime.Parse("2020-01-01")
            }
        });

        //Act
        await _deploymentService.Process();

        if (environment == "")
        {
            _logger.Assert(LogLevel.Debug, "Skipping ignored environment [Environment: {IgnoredEnvironment}]");
            _logger.Assert(LogLevel.Debug, $"Skipping ignored environment [Environment: {rawEnvironment.ToUpper()}]");
        }
        else
        {
            if (environment != null)
            {
                Assert.Equal(1, _storedDeployments.Count);

                Assert.Equal(environment, _storedDeployments.First().Environment);
            }
            else
            {
                Assert.Equal(0, _storedDeployments.Count);

                _logger.Assert(LogLevel.Warning,
                    "Could not translate environment [Untranslated Environment: {UntranslatedEnvironment}, Link:{webLink}]");
                _logger.Assert(LogLevel.Warning,
                    $"Could not translate environment [Untranslated Environment: {rawEnvironment.ToUpper()}, Link:(null)]");
            }
        }
    }
}