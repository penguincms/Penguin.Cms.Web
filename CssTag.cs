﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace Penguin.Cms.Web
{
    public class CssTag : IncludeTag
    {
        public CssTag(string path, IHtmlHelper helper, string version = null) : base(path, helper, "css", version)
        {
        }

        public override string ToString() => $"<link href=\"/{this.Url}?BuildVersion={this.Version}\" rel=\"stylesheet\" />";
    }
}
