using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Threading.Tasks;

namespace ApsCache
{
    public class ApsCacheHandler
    {
        private readonly ILogger _logger;

        public ApsCacheHandler(ILogger<ApsCacheHandler> logger,RequestDelegate next)
        {
            this._logger=logger;
            // This is an HTTP Handler, so no need to store next
        }

        public async Task Invoke(HttpContext context)
        {
            string response = GenerateResponse(context);

            context.Response.ContentType = GetContentType();
            await context.Response.WriteAsync(response);
        }

        // ...

        private string GenerateResponse(HttpContext context)
        {
            HttpRequest request= context.Request;


            string ep= "http://dccbrt.com";
            IRestClient rc = new RestClient(ep);
            IRestRequest req = new RestRequest("/");

            IRestResponse resp = rc.Get(req);


            var config = Configuration.Default;
            var browsingContext = BrowsingContext.New(config);

            //Create a virtual request to specify the document to load...
            //TODO: you have got to be kidding me...there HAS to be some sort of way to  
            //just 'gin up the IDoc object from a static string?
            IDocument doc = browsingContext.OpenAsync(req => req.Content(resp.Content)).Result;

            if(doc!=null){
                //TODO: validate we have the full content, and not that fecking JS required error...

                //look at the entire body:
                //Console.WriteLine(doc.DocumentElement.OuterHtml);                

                //This gives you the entire table of items...
                //IHtmlCollection<IElement> torrentList= doc.QuerySelectorAll("ol#torrents");

                var div = doc.CreateElement("div");
                div.TextContent = string.Format("Injected Additional Content:  original Path: {0}",request.Path);
                doc.Body.AppendChild(div);

            }else{
                _logger.LogWarning("No Content Found");
            }

            return  doc.Body.OuterHtml;
        }

        private string GetContentType()
        {
            return "text/html";
        }
    }

    public static class ApsCacheHandlerExtensions
    {
        public static IApplicationBuilder UseApsCacheHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApsCacheHandler>();
        }
    }
}