namespace Application.Deployments;

public class ValidEnvironments(Dictionary<string, string> dictionary)
{
    public string GetEnvironment(string rawEnvironment)
    {
        var match = dictionary.FirstOrDefault(m =>
            rawEnvironment.Contains(m.Key, StringComparison.InvariantCultureIgnoreCase));

        return match.Value;
    }
}