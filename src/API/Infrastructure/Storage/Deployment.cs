namespace API.Infrastructure.Storage;

public record Deployment(
    string id,
    string environment,
    string teamname,
    DateTime deploymenttime
);