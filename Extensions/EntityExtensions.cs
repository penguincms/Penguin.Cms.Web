using Penguin.Cms.Entities;
using System;

namespace Penguin.Cms.Web.Extensions
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class EntityExtensions
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        /// <summary>
        /// Guid.Empty if entity is null, else Guid
        /// </summary>
        /// <param name="e">The entity to check</param>
        /// <returns>Guid.Empty if entity is null, else Guid</returns>
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