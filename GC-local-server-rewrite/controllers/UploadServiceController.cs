using System.Net.Mime;
using System.Text;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace GCLocalServerRewrite.controllers;

public class UploadServiceController : WebApiController
{
    [Route(HttpVerbs.Post, "/upload.php")]
    // ReSharper disable once UnusedMember.Global
    public string UploadService()
    {
        HttpContext.Response.ContentType = MediaTypeNames.Text.Plain;
        HttpContext.Response.ContentEncoding = new UTF8Encoding(false);
        HttpContext.Response.KeepAlive = true;

        return "1\n" +
               "OK";
    }
}