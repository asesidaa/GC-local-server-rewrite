using System.Net.Mime;
using System.Text;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace GCLocalServerRewrite.controllers;

public class IncomServiceController : WebApiController
{
    [Route(HttpVerbs.Post, "/incom.php")]
    // ReSharper disable once UnusedMember.Global
    public string IncomService()
    {
        HttpContext.Response.ContentType = MediaTypeNames.Text.Plain;
        HttpContext.Response.ContentEncoding = Encoding.Default;
        HttpContext.Response.KeepAlive = true;

        return "1+1";
    }

    [Route(HttpVerbs.Post, "/incomALL.php")]
    // ReSharper disable once UnusedMember.Global
    public string IncomAllService()
    {
        HttpContext.Response.ContentType = MediaTypeNames.Text.Plain;
        HttpContext.Response.ContentEncoding = Encoding.Default;
        HttpContext.Response.KeepAlive = true;

        return "1+1";
    }
}