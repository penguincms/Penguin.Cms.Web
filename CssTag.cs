using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.Cms.Web
{
    public class CssTag : IncludeTag
    {
        public CssTag(string path, IHtmlHelper helper, string version = null) : base(path, helper, "css", version)
        {
        }

        public override string ToString()
        {
            return $"<link href=\"/{this.Url}?BuildVersion={this.Version}\" rel=\"stylesheet\" />";
        }
    }
}
