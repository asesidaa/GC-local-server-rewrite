using System.Net.Mime;
using System.Text;
using System.Xml;
using ChoETL;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using GCLocalServerRewrite.common;
using GCLocalServerRewrite.models;
using Swan;
using Swan.Logging;
// ReSharper disable UnusedMember.Global

namespace GCLocalServerRewrite.controllers;

public class RankController : WebApiController
{
    [Route(HttpVerbs.Get, "/ranking.php")]
    public string Rank([QueryField("cmd_type")] int type)
    {
        HttpContext.Response.ContentType = MediaTypeNames.Application.Octet;
        HttpContext.Response.ContentEncoding = Encoding.Default;
        HttpContext.Response.KeepAlive = true;

        if (!Enum.IsDefined(typeof(RankType), type))
        {
            throw new ArgumentOutOfRangeException(nameof(type));
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

                return ConstructResponse(RankTemp("play_num_rank"));
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