using Microsoft.AspNetCore.Http;
using System;

namespace Penguin.Cms.Web.Extensions
{
    public static class HttpContextExtensions
    {
        public static ISession? TryGetSession(this HttpContext context)
        {
            try
            {
                return context?.Session;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}