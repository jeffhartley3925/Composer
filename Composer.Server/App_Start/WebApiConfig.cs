using System.Web.Http;

class WebApiConfig
{
    public static void Register(HttpConfiguration configuration)
    {

        configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; 

        configuration.Routes.MapHttpRoute("API Default", "api/{controller}/{id}",
            new { id = RouteParameter.Optional });
    }
}