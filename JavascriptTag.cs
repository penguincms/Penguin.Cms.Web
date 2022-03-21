using Microsoft.AspNetCore.Mvc.Rendering;

namespace Penguin.Cms.Web
{
    public class JavascriptTag : IncludeTag
    {

        public JavascriptTag(string path, IHtmlHelper helper, string version = null) : base(path, helper, "js", version)
        {

        }

        public override string ToString() => $"<script src=\"/{this.Url}?BuildVersion={this.Version}\"></script>";
    }
}
