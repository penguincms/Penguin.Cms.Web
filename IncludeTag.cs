using Microsoft.AspNetCore.Mvc.Rendering;
using Penguin.Cms.Web.Extensions;
using System;
using System.Globalization;

namespace Penguin.Cms.Web
{
    public abstract class IncludeTag
    {
        protected string Url { get; private set; }
        protected IHtmlHelper Helper { get; private set; }
        protected string Version { get; private set; }
        public bool Exists => Helper.UrlExists(Url);

        protected IncludeTag(string path, IHtmlHelper helper, string extension, string? version = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace.", nameof(path));
            }

            if (helper is null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            Helper = helper;

            Version = version ?? DateTime.Now.ToString("yyyyMMddhhmm", CultureInfo.CurrentCulture);

            path = !path.StartsWith("/", StringComparison.OrdinalIgnoreCase) ? $"{extension}/{path}" : path[1..];

            Url = $"{path}.{extension}";
        }
    }
}