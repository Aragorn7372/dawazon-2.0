    using dawazon2._0.Infraestructures;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace dawazonTest.dawazon2._0.Infraestructure;

[TestFixture]
public class SerilogConfigTests
{
    [Test]
    public void Configure_ShouldReturnLoggerConfiguration()
    {
        var loggerConfig = SerilogConfig.Configure();

        Assert.That(loggerConfig, Is.Not.Null);
        Assert.That(loggerConfig, Is.TypeOf<LoggerConfiguration>());

        using var logger = loggerConfig.CreateLogger();
        
        Assert.That(logger, Is.Not.Null);
        Assert.That(logger.IsEnabled(LogEventLevel.Information), Is.True);
    }
}
