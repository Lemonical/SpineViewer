using Microsoft.Extensions.Logging.Abstractions;
using SpineViewer.Infrastructure.Diagnostics;
using Xunit;

namespace SpineViewer.Infrastructure.Tests;

public sealed class MicrosoftExtensionsAppLoggerTests
{
    [Fact]
    public void LoggingMethods_DoNotThrow_WhenLoggerIsAvailable()
    {
        MicrosoftExtensionsAppLogger<MicrosoftExtensionsAppLoggerTests> logger = new(
            new NullLogger<MicrosoftExtensionsAppLoggerTests>());

        logger.LogInformation("info");
        logger.LogWarning("warning");
        logger.LogError("error", new InvalidOperationException("boom"));
    }
}
