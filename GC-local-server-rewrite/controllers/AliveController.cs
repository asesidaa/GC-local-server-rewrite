using System.Net;
using System.Net.Mime;
using System.Text;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace GCLocalServerRewrite.controllers;

public class AliveController : WebApiController
{
    [Route(HttpVerbs.Get, "/i.php")]
    // ReSharper disable once UnusedMember.Global
    public string Check()
    {
        HttpContext.Response.ContentType = MediaTypeNames.Text.Html;
        HttpContext.Response.ContentEncoding = new UTF8Encoding(false);
        HttpContext.Response.KeepAlive = true;

        return "REMOTE ADDRESS: 127.0.0.1\n" +
               "SERVER NAME:nesys.home\n" +
               "SERVER ADDR:239.1.1.1";
    }

    [Route(HttpVerbs.Get, "/{id}/Alive.txt")]
    // ReSharper disable once UnusedMember.Global
    public void AliveFile()
    {
        HttpContext.Response.SetEmptyResponse((int)HttpStatusCode.OK);
        HttpContext.Response.ContentType = MediaTypeNames.Text.Plain;
        HttpContext.Response.ContentEncoding = new UTF8Encoding(false);
        HttpContext.Response.KeepAlive = true;
    }
}