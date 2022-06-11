using System.Collections.Specialized;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using GCLocalServerRewrite.common;
using Swan.Logging;

// ReSharper disable UnusedMember.Global

namespace GCLocalServerRewrite.controllers;

public class ServerController : WebApiController
{
    [Route(HttpVerbs.Get, "/certify.php")]
    public string Certify([QueryData] NameValueCollection parameters)
    {
        HttpContext.Response.ContentType = MediaTypeNames.Text.Plain;
        HttpContext.Response.ContentEncoding = new UTF8Encoding(false);
        HttpContext.Response.KeepAlive = true;

        var gid = parameters["gid"];
        var mac = parameters["mac"];
        var random = parameters["r"];
        var hash = parameters["md"];

        if (gid == null)
        {
            return QuitWithError(ErrorCode.ErrorNoGid);
        }

        if (mac == null)
        {
            return QuitWithError(ErrorCode.ErrorNoMac);
        }

        if (random == null)
        {
            return QuitWithError(ErrorCode.ErrorNoRandom);
        }

        if (hash == null)
        {
            return QuitWithError(ErrorCode.ErrorNoHash);
        }

        if (!MacValid(mac))
        {
            return QuitWithError(ErrorCode.ErrorInvalidMac);
        }

        if (!Md5Valid(hash))
        {
            return QuitWithError(ErrorCode.ErrorInvalidHash);
        }

        using var md5 = MD5.Create();

        var ticket = string.Join(string.Empty,
            md5.ComputeHash(Encoding.UTF8.GetBytes(gid)).Select(b => b.ToString("x2")));
        

        var response = $"host=card_id=7020392000147361,relay_addr={Configs.SETTINGS.RelayServer},relay_port={Configs.SETTINGS.RelayPort}\n" +
                       "no=1337\n" +
                       "name=123\n" +
                       "pref=nesys\n" +
                       "addr=nesys@home\n" +
                       "x-next-time=15\n" +
                       $"x-img=http://{Configs.SETTINGS.ServerIp}{Configs.STATIC_BASE_ROUTE}/news.png\n" +
                       $"x-ranking=http://{Configs.SETTINGS.ServerIp}{Configs.RANK_BASE_ROUTE}/ranking.php\n" +
                       $"ticket={ticket}";

        return response;
    }

    [Route(HttpVerbs.Get, "/cursel.php")]
    public string Cursel()
    {
        HttpContext.Response.ContentType = MediaTypeNames.Text.Plain;
        HttpContext.Response.ContentEncoding = new UTF8Encoding(false);
        HttpContext.Response.KeepAlive = true;

        return "1\n";
    }

    [Route(HttpVerbs.Get, "/data.php")]
    public string Data()
    {
        HttpContext.Response.ContentType = MediaTypeNames.Text.Plain;
        HttpContext.Response.ContentEncoding = new UTF8Encoding(false);
        HttpContext.Response.KeepAlive = true;

        var responseList = Configs.SETTINGS.ResponseData.ToList();
        var count = responseList.Count;
        var dataString = new StringBuilder();
        for (var i = 0; i < count; i++)
        {
            var data = responseList[i];
            dataString.Append($"{i},{data.FileName},{data.NotBeforeUnixTime},{data.NotAfterUnixTime},{data.Md5},{data.Index}");
            dataString.Append('\n');
        }

        return $"count={count}\n" +
               "nexttime=0\n" +
               dataString.ToString().TrimEnd('\n');
    }

    [Route(HttpVerbs.Get, "/gameinfo.php")]
    public string GameInfo()
    {
        HttpContext.Response.ContentType = MediaTypeNames.Text.Plain;
        HttpContext.Response.ContentEncoding = new UTF8Encoding(false);
        HttpContext.Response.KeepAlive = true;

        return "0\n" +
               "3\n" +
               "301000,test1\n" +
               "302000,test2\n" +
               "303000,test3";
    }

    private static bool MacValid(string mac)
    {
        return Regex.IsMatch(mac, "^[a-fA-F0-9]{12}$");
    }

    private static bool Md5Valid(string md5)
    {
        return Regex.IsMatch(md5, "^[a-fA-F0-9]{32}$");
    }

    private static string QuitWithError(ErrorCode errorCode)
    {
        return $"error={(int)errorCode}";
    }

    private enum ErrorCode
    {
        ErrorNoGid,
        ErrorNoMac,
        ErrorNoRandom,
        ErrorNoHash,
        ErrorInvalidMac,
        ErrorInvalidHash
    }
}