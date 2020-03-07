using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Penguin.Cms.Web.Mvc.Middleware
{
    //https://exceptionnotfound.net/using-middleware-to-log-requests-and-responses-in-asp-net-core/
    public class ConfigurePersistenceMiddleware
    {
        private readonly RequestDelegate _next;

        public ConfigurePersistenceMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string SetupUrl = "/Admin/Setup/";

            if (context.Request.Path.ToString().StartsWith(SetupUrl, System.StringComparison.OrdinalIgnoreCase))
            {
                await this._next(context);
            }
            else
            {
                context.Response.Redirect(SetupUrl);
            }
        }
    }
}