namespace API.Infrastructure.Gateways;

public interface IDateTimeFactory
{
    DateTime GetCurrentUTC();
}