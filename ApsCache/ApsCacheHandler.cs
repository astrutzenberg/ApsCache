using AngleSharp;
using AngleSharp.Dom;
using ApsCache.ConfigOptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Extensions;
using System.IO;
using System.Threading.Tasks;

namespace ApsCache
{
    public class ApsCacheHandler
    {
        private readonly ILogger _logger;
        private readonly IApsSettings _apsSettings;

        public ApsCacheHandler(IOptions<ApsSettings> options,ILogger<ApsCacheHandler> logger,RequestDelegate next)
        {
            this._logger=logger;
            this._apsSettings = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            IRestResponse upstreamResponse = GenerateResponse(context);

            Microsoft.AspNetCore.Http.HttpResponse cResponse = context.Response;

            cResponse.ContentType = upstreamResponse.ContentType;
            cResponse.StatusCode = (int)upstreamResponse.StatusCode;
            await context.Response.WriteAsync(upstreamResponse.Content);
        }

        // ...

        private IRestResponse GenerateResponse(HttpContext context)
        {
            HttpRequest request= context.Request;

            _logger.LogInformation("Upstream Request: {0}, Relative Uri: {1} ",request.Method, request.GetEncodedPathAndQuery());

            string ep = _apsSettings.Endpoint;
            IRestClient rc = new RestClient(ep);

            IRestRequest req = new RestRequest(request.GetEncodedPathAndQuery());
            if (request.Body != null)
            {
                using (StreamReader rdr = new StreamReader(request.Body))
                {
                    string body = rdr.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        _logger.LogInformation("Body: {0}", body);
                        req.AddParameter("body", body);
                    }
                }
            }

            if(request.Cookies.Count > 0)
            {
                foreach(string key in request.Cookies.Keys)
                {
                    req.AddCookie(key, request.Cookies[key]);
                }
            }

            //inject other headers request headers
            /*
            if (request.Headers.Count > 0)
            {
                foreach (string key in request.Headers.Keys)
                {
                    //req.AddHeader(key, request.Headers[key]);
                }
            }
            */

            IRestResponse resp = null;
            switch (request.Method.ToLower())
            {
                case "get":

                    resp = rc.Get(req);
                    break;
                case "post":
                    resp = rc.Post(req);
                    break;
                case "put":
                    resp = rc.Put(req);
                    break;
                case "delete":
                    resp = rc.Delete(req);
                    break;
                case "head":
                    resp = rc.Head(req);
                    break;
            }

            return resp;

            //If we need to inject info into the response content?
            /*
            var config = Configuration.Default;
            var browsingContext = BrowsingContext.New(config);

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
            */
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