using Application.Interfaces;
using Domain.Config;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace XmlSerializationTests;

public class EventManagerServiceTests
{
    [Fact]
    public void InitializeEvents_FileOnlyInRegisteredProvider_LoadsAndHashesFile()
    {
        var providerRoot = Path.Combine(Path.GetTempPath(), "gc-local-server-tests", Guid.NewGuid().ToString("N"));
        var eventsDirectory = Path.Combine(providerRoot, "events");
        var eventFileName = $"news_big_{Guid.NewGuid():N}.jpg";
        Directory.CreateDirectory(eventsDirectory);
        File.WriteAllBytes(Path.Combine(eventsDirectory, eventFileName), [1, 2, 3, 4]);

        try
        {
            using var fileProvider = new PhysicalFileProvider(providerRoot);
            var services = new ServiceCollection();
            services.AddLogging();
            services.Configure<EventConfig>(config =>
            {
                config.UseEvents = true;
                config.EventFiles.Add(new EventFile
                {
                    FileName = eventFileName,
                    Index = 0
                });
            });
            services.AddSingleton<IFileProvider>(fileProvider);
            services.AddSingleton<IEventManagerService, EventManagerService>();

            using var serviceProvider = services.BuildServiceProvider();
            var eventManager = serviceProvider.GetRequiredService<IEventManagerService>();

            eventManager.InitializeEvents();

            var eventData = Assert.Single(eventManager.GetEvents());
            Assert.Equal(eventFileName, eventData.Name);
            Assert.Equal("08d6c05a21512a79a1dfeb9d2a8f262f", eventData.Md5);
        }
        finally
        {
            Directory.Delete(providerRoot, recursive: true);
        }
    }
}
