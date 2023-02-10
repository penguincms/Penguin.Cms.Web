using Penguin.Extensions.Collections;
using Penguin.Persistence.Abstractions.Attributes.Rendering;
using Penguin.Persistence.Abstractions.Attributes.Validation;
using Penguin.Reflection.Serialization.Abstractions.Interfaces;
using Penguin.Reflection.Serialization.Extensions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Penguin.Cms.Web.Extensions
{
    public static class IMetaPropertyExtensions
    {
        public static string DisplayName(this IMetaProperty property)
        {
            if (property is null)
            {
                throw new System.ArgumentNullException(nameof(property));
            }

            string display = property.AttributeRef<DisplayAttribute, string>(nameof(DisplayAttribute.Name));

            return string.IsNullOrWhiteSpace(display) ? property.Name : display;
        }

        public static T Max<T>(T a, T b)
        {
            return Comparer<T>.Default.Compare(a, b) >= 0 ? a : b;
        }

        public static T MaxValue<T>(this IMetaProperty p, T Default)
        {
            if (p is null)
            {
                throw new System.ArgumentNullException(nameof(p));
            }

            if (p.Attributes.AnyNotNull(a => a.Type.Name == typeof(RangeAttribute).Name))
            {
                T val = Default;

                foreach (T checkVal in p.Attributes.Where(a => a.Type.Name == typeof(RangeAttribute).Name).Select(a => a[nameof(RangeAttribute.Maximum)].GetValue<T>()))
                {
                    val = Min(val, checkVal);
                }

                return val;
            }
            else
            {
                return Default;
            }
        }

        public static T MaxValue<T>(this IMetaProperty p)
        {
            if (p is null)
            {
                throw new System.ArgumentNullException(nameof(p));
            }

            if (p.Attributes.AnyNotNull(a => a.Type.Name == typeof(RangeAttribute).Name))
            {
                if (typeof(T).InvokeMember("MaxValue", BindingFlags.GetField, null, null, null, CultureInfo.CurrentCulture) is T val)
                {
                    foreach (T checkVal in p.Attributes.Where(a => a.Type.Name == typeof(RangeAttribute).Name).Select(a => a[nameof(RangeAttribute.Maximum)].GetValue<T>()))
                    {
                        val = Min(val, checkVal);
                    }

                    return val;
                }
            }
            else
            {
                if (typeof(T).InvokeMember("MaxValue", BindingFlags.GetField, null, null, null, CultureInfo.CurrentCulture) is T val)
                {
                    return val;
                }
            }

            throw new System.Exception("Somehow we failed to find a non-null max value for the requested type");
        }

        public static T Min<T>(T a, T b)
        {
            return Comparer<T>.Default.Compare(a, b) >= 0 ? b : a;
        }

        public static T MinValue<T>(this IMetaProperty p, T Default)
        {
            if (p is null)
            {
                throw new System.ArgumentNullException(nameof(p));
            }

            if (p.Attributes.AnyNotNull(a => a.Type.Name == typeof(RangeAttribute).Name))
            {
                T val = Default;

                foreach (T checkVal in p.Attributes.Where(a => a.Type.Name == typeof(RangeAttribute).Name).Select(a => a[nameof(RangeAttribute.Minimum)].GetValue<T>()))
                {
                    val = Max(val, checkVal);
                }

                return val;
            }
            else
            {
                return Default;
            }
        }

        public static T MinValue<T>(this IMetaProperty p)
        {
            if (p is null)
            {
                throw new System.ArgumentNullException(nameof(p));
            }

            if (p.Attributes.AnyNotNull(a => a.Type.Name == typeof(RangeAttribute).Name))
            {
                if (typeof(T).InvokeMember("MinValue", BindingFlags.GetField, null, null, null, CultureInfo.CurrentCulture) is T val)
                {
                    foreach (T checkVal in p.Attributes.Where(a => a.Type.Name == typeof(RangeAttribute).Name).Select(a => a[nameof(RangeAttribute.Minimum)].GetValue<T>()))
                    {
                        val = Max(val, checkVal);
                    }

                    return val;
                }
            }
            else
            {
                if (typeof(T).InvokeMember("MinValue", BindingFlags.GetField, null, null, null, CultureInfo.CurrentCulture) is T val)
                {
                    return val;
                }
            }

            throw new System.Exception("Somehow we failed to find a non-null min value for the requested type");
        }
    }
}