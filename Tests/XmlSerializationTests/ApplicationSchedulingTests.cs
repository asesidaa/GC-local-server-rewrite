using Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace XmlSerializationTests;

public class ApplicationSchedulingTests
{
    [Fact]
    public void AddApplication_DoesNotScheduleObsoleteMaintainNullValuesJob()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<QuartzOptions>>().Value;

        Assert.DoesNotContain(options.JobDetails,
            job => job.Key.Name == "MaintainNullValuesJob");
        Assert.DoesNotContain(options.Triggers,
            trigger => trigger.Key.Name == "MaintainNullValuesJob-trigger");
    }
}
