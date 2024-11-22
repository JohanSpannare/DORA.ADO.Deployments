using API.Deployments;
using API.Infrastructure.Gateways;
using API.Infrastructure.Storage;
using API.Infrastructure.Tasks;
using Application.Deployments;
using Domain;
using Domain.Deployments;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<DeploymentBackgroundService>();
builder.Services.AddLogging();

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddAuthentication().AddJwtBearer();

builder.Services.AddSingleton<IDateTimeFactory, DateTimeFactory>();
builder.Services.AddTransient<DeploymentService>();
builder.Services.AddTransient<IGetDeployments, AzureGateway>();
builder.Services.AddTransient<IGetProjects, AzureGateway>();
builder.Services.AddTransient<IGetTeams, AzureGateway>();


//builder.Services.AddDbContextFactory<DeploymentContext>((provider, options) =>
//{
//    var deploymentContextSettings = provider.GetRequiredService<DeploymentContextSettings>();

//    options.UseSqlServer(deploymentContextSettings.ConnectionString);
//});

//builder.Services.AddTransient<IDeploymentStorage, MSSql>();
builder.Services.AddSingleton<IDeploymentStorage, Memory>();


builder.Services.AddSingleton<DeploymentContextSettings>(provider =>
{
    var deploymentContextSettings = new DeploymentContextSettings();

    provider.GetRequiredService<IConfiguration>().GetSection(nameof(DeploymentContextSettings))
        .Bind(deploymentContextSettings);

    return deploymentContextSettings;
});


builder.Services.AddTransient<IVssConnection>(provider =>
{
    var configurationSection = provider.GetRequiredService<IConfiguration>().GetSection(nameof(VssConnection));
    var accessToken = configurationSection["AccessToken"];
    var url = configurationSection["URL"];

    var vssOAuthAccessToken = new VssOAuthAccessTokenCredential(accessToken);
    var connection = new VssConnectionWrapper(new Uri(url),
        vssOAuthAccessToken);
    return connection;
});


builder.Services.AddSingleton<ITaskRunner, TaskRunner>();


builder.Services.AddSingleton<DeploymentServiceSettings>(provider =>
{
    var deploymentServiceSettings = new DeploymentServiceSettings();

    provider.GetRequiredService<IConfiguration>().GetSection(nameof(DeploymentServiceSettings))
        .Bind(deploymentServiceSettings);

    return deploymentServiceSettings;
});

builder.Services.AddSingleton<ValidEnvironments>(provider =>
{
    var dictionary = new Dictionary<string, string>();

    provider.GetRequiredService<IConfiguration>().GetSection(nameof(ValidEnvironments)).Bind(dictionary);

    return new ValidEnvironments(dictionary);
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/healthcheck");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers().AllowAnonymous();

app.Run();

namespace API
{
    public class Program
    {
    }
}