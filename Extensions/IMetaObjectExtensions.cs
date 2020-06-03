using Penguin.Persistence.Abstractions;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Persistence.Repositories.Interfaces;
using Penguin.Reflection.Serialization.Abstractions.Interfaces;
using Penguin.Reflection.Serialization.Extensions;
using System;

namespace Penguin.Cms.Web.Extensions
{
    public static class IMetaObjectExtensions
    {
        public static T FromDatabase<T>(this IMetaObject o, IServiceProvider serviceProvider) where T : KeyedObject
        {
            if (o is null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            IKeyedObjectRepository<T> repo = (IKeyedObjectRepository<T>)serviceProvider.GetService(typeof(IRepository<T>));

            return repo.Find(o[nameof(KeyedObject._Id)].GetValue<int>());
        }

        public static IMetaObject GetRoot(this IMetaObject o)
        {
            if (o is null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            IMetaObject parent = o;

            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }

            return parent!;
        }

        public static bool IsRoot(this IMetaObject o)
        {
            if (o is null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            return o.Parent is null;
        }
    }
}