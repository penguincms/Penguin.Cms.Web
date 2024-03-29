using Penguin.Cms.Entities;
using Penguin.Persistence.Abstractions;
using Penguin.Persistence.Abstractions.Attributes.Control;
using Penguin.Reflection.Serialization.Abstractions.Interfaces;
using Penguin.Reflection.Serialization.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Penguin.Cms.Web.Extensions
{
    public class ContextHelper
    {
        private const string KEY_NOT_FOUND_MESSAGE = "Key not found. Logic might need to be added to find a key based on attribute";

        protected IServiceProvider ServiceProvider { get; set; }

        public ContextHelper(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms")]
        public static Guid GetGuid(object o)
        {
            if (o is null)
            {
                return Guid.Empty;
            }

            if (o is Entity e)
            {
                return e.Guid;
            }
            string? KeyValue = GetKeyForType(o.GetType())?.GetValue(o)?.ToString();

            if (KeyValue is null)
            {
                throw new Exception($"Unable to find key value for type {o.GetType()}");
            }

            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes($"{o.GetType().FullName}_{KeyValue}"));
            return new Guid(hash);
        }

        public static Guid GetGuid(IMetaObject o)
        {
            if (o is null)
            {
                return Guid.Empty;
            }

            if (o.Type.Is(typeof(Entity)))
            {
                return Guid.Parse(o["Guid"].Value);
            }

            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes($"{o.Type.FullName}_{o[GetKeyForType(o.Type)].Value}"));
            return new Guid(hash);
        }

        public static PropertyInfo? GetKeyForType(System.Type t, bool throwError = true)
        {
            if (t is null)
            {
                throw new ArgumentNullException(nameof(t));
            }

            if (t.IsSubclassOf(typeof(KeyedObject)))
            {
                return typeof(KeyedObject).GetProperty(nameof(KeyedObject._Id));
            }
            else
            {
                return t.GetProperties().Any(p => string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase))
                    ? t.GetProperties().First(p => string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase))
                    : throwError ? throw new Exception(KEY_NOT_FOUND_MESSAGE) : null;
            }
        }

        //This should be merged with the function that takes an IType
        //but in order to do so, property attribute information needs to be added in the reflector
        //for the type object itself, which its not. It wouldn't be hard but this is a quick fix
        public static string? GetKeyForType(IMetaObject o, bool throwError = true)
        {
            return o is null
                ? throw new ArgumentNullException(nameof(o))
                : o.Properties != null && o.Properties.Any(p => p.Property.HasAttribute<KeyAttribute>())
                ? o.Properties.First(p => p.Property.HasAttribute<KeyAttribute>()).Property.Name
                : GetKeyForType(o.Type, throwError);
        }

        public static string? GetKeyForType(IMetaType t, bool throwError = true)
        {
            if (t is null)
            {
                throw new ArgumentNullException(nameof(t));
            }

            if (t.Is(typeof(KeyedObject)))
            {
                return nameof(KeyedObject._Id);
            }
            else
            {
                return t.Properties.Any(p => string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase))
                    ? t.Properties.First(p => string.Equals(p.Name, "id", StringComparison.OrdinalIgnoreCase)).Name
                    : throwError ? throw new Exception(KEY_NOT_FOUND_MESSAGE) : null;
            }
        }

        public static bool IsKey(IMetaObject o)
        {
            return o?.Parent is IMetaObject parent && o.Property.Name == GetKeyForType(parent, false);
        }
    }
}