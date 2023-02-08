using System.Security.Cryptography;
using Application.Interfaces;
using Domain.Config;
using Domain.Models;
using Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class EventManagerService : IEventManagerService
{
    #region Constants

    private const string WWWROOT = "wwwroot";

    private const string EVENT_FOLDER = "events";

    private static readonly DateTimeOffset NOT_BEFORE = new(new DateTime(2013, 1, 1));

    private static readonly DateTimeOffset NOT_AFTER = NOT_BEFORE + new TimeSpan(360 * 20, 0, 0, 0);

    private static readonly string NOT_BEFORE_STRING = NOT_BEFORE.ToUnixTimeSeconds().ToString();

    private static readonly string NOT_AFTER_STRING = NOT_AFTER.ToUnixTimeSeconds().ToString();

    #endregion

    private readonly ILogger<EventManagerService> logger;
    
    private readonly EventConfig config;

    private readonly List<Event> events;

    private readonly bool useEvents;

    public EventManagerService(IOptions<EventConfig> config, ILogger<EventManagerService> logger)
    {
        this.logger = logger;
        this.config = config.Value;
        events = new List<Event>();
        useEvents = this.config.UseEvents;
    }

    public bool UseEvents()
    {
        return useEvents;
    }

    public IEnumerable<Event> GetEvents()
    {
        return events;
    }

    public void InitializeEvents()
    {
        foreach (var eventData in config.EventFiles)
        {
            var filePath = Path.Combine(WWWROOT, EVENT_FOLDER, eventData.FileName);
            if (!File.Exists(filePath))
            {
                logger.LogError("Event file {File} not found at path {Path}!", eventData.FileName,
                    filePath);
                throw new EventFileNotFoundException();
            }

            var md5 = ComputeFileMd5(filePath);
            var @event = new Event
            {
                Name = eventData.FileName,
                Md5 = md5,
                NotBefore = NOT_BEFORE_STRING,
                NotAfter = NOT_AFTER_STRING
            };

            var eventType = DetermineFileType(eventData.FileName);
            @event.Index = eventType switch
            {
                EventFileType.Event => 0,
                EventFileType.EventRegPic => 1,
                EventFileType.EventSgRegPic => 2,
                EventFileType.NewsBigPic => eventData.Index,
                EventFileType.NewsSmallPic => 1,
                EventFileType.Telop => 0,
                EventFileType.EventCmp => 8,
                _ => throw new ArgumentOutOfRangeException()
            };
            events.Add(@event);
        }

        if (events.Exists(event1 => event1.Name.StartsWith("news_big_") && event1.Index == 0))
        {
            return;
        }

        logger.LogWarning("No big news image with index 0! Changing a random one...");
        events.First(event1 => event1.Name.StartsWith("news_big_")).Index = 0;
    }

    private EventFileType DetermineFileType(string fileName)
    {
        if (fileName.EndsWith(".evt"))
        {
            return EventFileType.Event;
        }

        if (fileName.EndsWith(".cmp"))
        {
            return EventFileType.EventCmp;
        }

        if (fileName.EndsWith(".txt"))
        {
            return EventFileType.Telop;
        }

        if (fileName.Contains("_reg"))
        {
            return EventFileType.EventRegPic;
        }

        if (fileName.Contains("_sgreg"))
        {
            return EventFileType.EventSgRegPic;
        }

        if (fileName.StartsWith("news_big_"))
        {
            return EventFileType.NewsBigPic;
        }

        if (fileName.StartsWith("news_small_"))
        {
            return EventFileType.NewsSmallPic;
        }

        logger.LogError("Unknown event file type for file {File}", fileName);
        throw new EventFileTypeUnknownException();
    }

    private static string ComputeFileMd5(string filePath)
    {
        using var file = File.OpenRead(filePath);
        var hash = MD5.HashData(file);
        var result = BitConverter.ToString(hash).Replace("-", "").ToLower();

        return result;
    }

    private enum EventFileType
    {
        Event,
        EventRegPic,
        EventSgRegPic,
        NewsBigPic,
        NewsSmallPic,
        Telop,
        EventCmp
    }
}