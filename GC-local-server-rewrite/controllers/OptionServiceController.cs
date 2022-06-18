using System.Net.Mime;
using System.Text;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using GCLocalServerRewrite.common;
using GCLocalServerRewrite.models;
using SQLite.Net2;
using Swan;
using Swan.Logging;

namespace GCLocalServerRewrite.controllers;

public class OptionServiceController : WebApiController
{

    private readonly SQLiteConnection cardSqLiteConnection;

    public OptionServiceController()
    {
        cardSqLiteConnection = DatabaseHelper.ConnectDatabase(Configs.SETTINGS.CardDbName);
    }

    [Route(HttpVerbs.Get, "/PlayInfo.php")]
    public string OptionService([QueryField("card_id")] long cardId)
    {

        HttpContext.Response.ContentType = MediaTypeNames.Text.Plain;
        HttpContext.Response.ContentEncoding = new UTF8Encoding(false);
        HttpContext.Response.KeepAlive = true;

        return "1\n" +
               $"{GetPlayCount(cardId)}";
    }

    private int GetPlayCount(long cardId)
    {
        var record = cardSqLiteConnection.Table<CardPlayCount>().Where(count => count.CardId == cardId);

        if (!record.Any())
        {
            return 0;
        }

        var now = DateTime.Now;
        var data = record.First();
        var lastPlayedTime = data.LastPlayed;

        if (now <= lastPlayedTime)
        {
            $"Current time {now} is less than or equal to last played time! Clock skew detected!".Warn();
            return 0;
        }

        DateTime start;
        DateTime end;
        if (now.Hour >= 8)
        {
            start = DateTime.Today.AddHours(8);
            end = start.AddHours(24);
        }
        else
        {
            end = DateTime.Today.AddHours(8);
            start = end.AddHours(-24);
        }

        return lastPlayedTime.IsBetween(start, end) ? data.PlayCount : 0;
    }
}