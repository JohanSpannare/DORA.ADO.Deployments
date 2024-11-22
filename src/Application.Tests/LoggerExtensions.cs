using System.Reflection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Application.Tests;

public static class LoggerExtensions
{
    public static void Assert(this ILogger logger, LogLevel logLevel, string message, int numberOfTimes = 1)
    {
        var receivedCalls = logger.ReceivedCalls();

        IEnumerable<(string formatedMessage, string message)> call = receivedCalls.Select(x =>
        {
            if (x.GetArguments()[0].Equals(logLevel))
            {
                var isFormatter = x.GetArguments()[2].GetType().FullName.Equals(
                    "Microsoft.Extensions.Logging.FormattedLogValues",
                    StringComparison.InvariantCultureIgnoreCase);

                string formatedMessage = null;
                string originalMessage = null;

                if (isFormatter)
                    formatedMessage = (string)x.GetArguments()[2].GetType()
                        .GetField("_originalMessage", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(x.GetArguments()[2]);

                originalMessage = x.GetArguments()[2].ToString();

                return (formatedMessage, originalMessage);
            }

            return default;
        });

        var notNullCalls = call.Where(x => x != default);

        var matchingMessages = notNullCalls.Where(x => x.formatedMessage.Equals(message) | x.message.Equals(message));

        var firstMatch = matchingMessages.FirstOrDefault();

        var allMessages = new HashSet<string>();

        foreach (var notNullCall in notNullCalls)
        {
            allMessages.Add(notNullCall.formatedMessage);
            allMessages.Add(notNullCall.message);
        }

        Xunit.Assert.True(firstMatch != default && matchingMessages.Count() == numberOfTimes,
            $"Expected {numberOfTimes} found {matchingMessages.Count()} Calls matching message '{message}' in messages: \r\n{string.Join("\r\n", allMessages)}");
    }
}