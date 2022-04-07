using System.Net.Mime;
using System.Text;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace GCLocalServerRewrite.controllers;

public class ResponeServiceController : WebApiController
{
    [Route(HttpVerbs.Post, "/respone.php")]
    // ReSharper disable once UnusedMember.Global
    public string ResponeService()
    {
        HttpContext.Response.ContentType = MediaTypeNames.Text.Plain;
        HttpContext.Response.ContentEncoding = new UTF8Encoding(false);
        HttpContext.Response.KeepAlive = true;

        return "1";
    }
}