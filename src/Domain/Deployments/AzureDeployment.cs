namespace Domain.Deployments;

public class AzureDeployment
{
    public string Id { get; set; }

    public string Stage { get; set; }
    public string UserId { get; set; }
    public DateTime Date { get; set; }
    public string? WebLink { get; set; }
}