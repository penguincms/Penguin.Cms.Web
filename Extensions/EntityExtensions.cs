using Penguin.Cms.Entities;
using System;

namespace Penguin.Cms.Web.Extensions
{
    public static class EntityExtensions
    {
        public static Guid TryGetGuid(this Entity e)
        {
            if (e is null)
            {
                return Guid.Empty;
            }
            else
            {
                return e.Guid;
            }
        }
    }
}