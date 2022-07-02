using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Swan.Logging;

namespace GCLocalServerRewrite.controllers
{
    // TODO: Add proper update check response
    public class UpdateController : WebApiController
    {
        [Route(HttpVerbs.Get, "/check.php")]
        public string CheckUpdate()
        {
            var parameters = HttpContext.GetRequestQueryData();
            foreach (var key in parameters.AllKeys)
            {
                $"Key {key}: {parameters[key]}".Info();
            }
            
            HttpContext.Response.StatusCode = 404;
            return "Not found";
        }
    }
}
