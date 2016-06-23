using System.Net.Http.Formatting;
using System.Web.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using ZipFormRest.Code;
using ZipFormRest.Controllers;

namespace ZipFormRest
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
           

            config.Routes.MapHttpRoute("Hooks", "hook/{transactionId}/{hookId}", new
            {
                controller = "hooks",
                //transactionId = RouteParameter.Optional,
                hookId = RouteParameter.Optional
               
            });



            //Optionally logs all requests and responses
            config.MessageHandlers.Add(new LogMessageHandler());
#if !DEBUG
            //In release mode only (production), enforces that the service is exposed on a SSL endpoint
            //config.MessageHandlers.Add(new SslCheckMessageHandler());
#endif
            //Ensure request content type is application/json, if not otherwise specified
            config.MessageHandlers.Add(new EnforceContentTypeMessageHandler());

            //Authenticates all requests
            config.MessageHandlers.Add(new AuthenticationMessageHandler());

            //Global error filter, logs the exception and returns a 500 Internal Server Error status code
            config.Filters.Add(new GlobalExceptionHandler());

            //Set the JSON serializer to always convert property names to camel-case
            JsonMediaTypeFormatter json = config.Formatters.JsonFormatter;
            //json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            json.SerializerSettings.Converters.Add(new StringEnumConverter());
            json.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            json.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

            XmlMediaTypeFormatter xml = config.Formatters.XmlFormatter;
            xml.UseXmlSerializer = true;

            //config.Formatters.JsonFormatter.MediaTypeMappings.Add(new UriPathExtensionMapping("json", "application/json"));
            //config.Formatters.XmlFormatter.MediaTypeMappings.Add(new UriPathExtensionMapping("xml", "application/xml"));

           // config.Filters.Add(new ValidateModelAttribute());
        }
    }
}