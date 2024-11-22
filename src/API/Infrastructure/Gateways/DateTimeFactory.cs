namespace API.Infrastructure.Gateways;

internal class DateTimeFactory : IDateTimeFactory
{
    public DateTime GetCurrentUTC()
    {
        return DateTime.UtcNow;
    }
}