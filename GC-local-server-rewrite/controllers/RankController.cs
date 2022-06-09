using System.Net.Mime;
using System.Text;
using System.Xml;
using ChoETL;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using GCLocalServerRewrite.common;
using GCLocalServerRewrite.models;
using SQLite.Net2;
using Swan;
using Swan.Logging;
// ReSharper disable UnusedMember.Global

namespace GCLocalServerRewrite.controllers;

public class RankController : WebApiController
{
    private readonly SQLiteConnection cardSqLiteConnection;
    private readonly SQLiteConnection musicSqLiteConnection;

    public RankController()
    {
        cardSqLiteConnection = DatabaseHelper.ConnectDatabase(Configs.SETTINGS.CardDbName);
        musicSqLiteConnection = DatabaseHelper.ConnectDatabase(Configs.SETTINGS.MusicDbName);
    }

    
    [Route(HttpVerbs.Get, "/ranking.php")]
    public string Rank([QueryField("cmd_type")] int type)
    {
        HttpContext.Response.ContentType = MediaTypeNames.Application.Octet;
        HttpContext.Response.ContentEncoding = new UTF8Encoding(false);
        HttpContext.Response.KeepAlive = true;

        if (!Enum.IsDefined(typeof(RankType), type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Rank type out of range");
        }

        var requestType = (RankType)type;

        switch (requestType)
        {
            case RankType.GlobalRank:
            case RankType.UnknownRank1:
            case RankType.UnknownRank2:
            case RankType.UnknownRank3:
                $"Getting rank request, type is {requestType}".Info();

                return ConstructResponse(RankTemp("score_rank"));
            case RankType.PlayNumRank:
                $"Getting rank request, type is {requestType}".Info();

                return ConstructResponse(PlayNumRank());
            case RankType.EventRank:
                $"Getting rank request, type is {requestType}".Info();

                return ConstructResponse(RankTemp("event_rank"));
            default:
#pragma warning disable CA2208
                throw new ArgumentOutOfRangeException(nameof(requestType));
#pragma warning restore CA2208
        }
    }

    private static string ConstructResponse(string xml)
    {
        return "1\n" +
               xml;
    }

    // TODO: Add proper rank support
    private static string RankTemp(string rankType)
    {
        var rankStatus = new RankStatus
        {
            Rows = 0,
            Status = 0
        };

        var sb = new StringBuilder();

        using (var writer = new ChoXmlWriter<RankStatus>(sb))
        {
            writer.Configuration.OmitXmlDeclaration = false;
            writer.Configuration.UseXmlSerialization = true;
            writer.WithXPath(Configs.RANK_STATUS_XPATH);

            writer.Write(rankStatus);
        }

        var document = new XmlDocument();
        document.LoadXml(sb.ToString());
        var root = document.DocumentElement;

        if (root == null)
        {
            throw SelfCheck.Failure("Internal XML error!");
        }

        root.AppendChild(document.CreateElement(rankType));

        var stringWriter = new StringWriter();
        var xmlTextWriter = new XmlTextWriter(stringWriter);
        document.WriteTo(xmlTextWriter);

        return stringWriter.ToString();
    }

    private string PlayNumRank()
    {
        var rankStatus = new RankStatus
        {
            Rows = 30,
            TableName = "play_num_rank",
            Status = 1
        };

        var playNumRankContainer = new PlayNumRankContainer
        {
            PlayNumRankRecords = GetPlayNumRankRecords(),
            RankStatus = rankStatus
        };

        var sb = new StringBuilder();

        using (var writer = new ChoXmlWriter<PlayNumRankContainer>(sb))
        {
            writer.Configuration.UseXmlSerialization = true;
            writer.Configuration.OmitXmlDeclaration = false;
            writer.Configuration.IgnoreRootName = true;
            writer.Write(playNumRankContainer);
        }

        return sb.ToString();
    }

    private List<PlayNumRankRecord> GetPlayNumRankRecords()
    {
        var records = new List<PlayNumRankRecord>();
        var musics = musicSqLiteConnection.Table<Music>().ToList().OrderBy(arg => Guid.NewGuid()).Take(30).ToList();

        for (var i = 0; i < musics.Count; i++)
        {
            var music = musics[i];
            var index = i + 1;
            var record = new PlayNumRankRecord
            {
                Id = index,
                Rank = index,
                Rank2 = index + 1,
                PrevRank = musics.Count - i,
                PrevRank2 = index + 1,
                Artist = music.Artist,
                Pcol1 = music.MusicId,
                ScoreBi1 = index,
                Title = music.Title
            };
            records.Add(record);
        }


        return records;
    }


    private enum RankType
    {
        GlobalRank = 4119,
        PlayNumRank = 6657,
        EventRank = 6661,
        UnknownRank1 = 6666,
        UnknownRank2 = 6667,
        UnknownRank3 = 4098
    }
}