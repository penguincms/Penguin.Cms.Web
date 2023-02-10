using Microsoft.AspNetCore.Mvc.Rendering;

namespace Penguin.Cms.Web
{
    public class CssTag : IncludeTag
    {
        public CssTag(string path, IHtmlHelper helper, string? version = null) : base(path, helper, "css", version)
        {
        }

        public override string ToString()
        {
            return $"<link href=\"/{Url}?BuildVersion={Version}\" rel=\"stylesheet\" />";
        }
    }
}