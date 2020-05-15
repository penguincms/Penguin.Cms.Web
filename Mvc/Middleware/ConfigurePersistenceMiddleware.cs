using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Penguin.Cms.Web.Mvc.Middleware
{
    //https://exceptionnotfound.net/using-middleware-to-log-requests-and-responses-in-asp-net-core/
    /// <summary>
    /// A class intended to catch a failed database configuration and return a setup page
    /// </summary>
    public class ConfigurePersistenceMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Constructs a new instance of this class
        /// </summary>
        /// <param name="next">The RequestDelegate</param>
        public ConfigurePersistenceMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        /// <summary>
        /// Invokes this middleware
        /// </summary>
        /// <param name="context">The current HttpContext</param>
        /// <returns>A task for the middleware execution</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

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