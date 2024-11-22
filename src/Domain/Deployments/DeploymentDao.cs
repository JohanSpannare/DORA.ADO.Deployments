namespace Domain.Deployments;

public class DeploymentDao
{
    public string Id { get; set; }
    public string Environment { get; set; }
    public string TeamName { get; set; }
    public DateTime Date { get; set; }
}