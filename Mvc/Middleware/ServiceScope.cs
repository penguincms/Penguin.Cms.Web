using Microsoft.AspNetCore.Http;
using Penguin.Web.DependencyInjection;
using System.Threading.Tasks;

namespace Penguin.Cms.Web.Mvc.Middleware
{
    //https://exceptionnotfound.net/using-middleware-to-log-requests-and-responses-in-asp-net-core/
    public class ServiceScope
    {
        private readonly RequestDelegate _next;

        public ServiceScope(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            using PerRequestServiceScope ServiceScope = new();
            ServiceScope.RequestProvider.Add(context);
            context.RequestServices = ServiceScope.ServiceProvider;

            await _next(context);
        }
    }
}