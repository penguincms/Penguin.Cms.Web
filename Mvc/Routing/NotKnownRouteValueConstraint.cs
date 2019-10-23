using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace Penguin.Cms.Web.Mvc.Routing
{
    public class NotKnownRouteValueConstraint : KnownRouteValueConstraint
    {
        public NotKnownRouteValueConstraint(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider) : base(actionDescriptorCollectionProvider)
        {
        }

        public new bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            return !base.Match(httpContext, route, routeKey, values, routeDirection);
        }
    }
}