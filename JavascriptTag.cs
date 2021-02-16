using Microsoft.AspNetCore.Mvc.Rendering;
using Penguin.Cms.Web.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Penguin.Cms.Web
{
    public class JavascriptTag : IncludeTag
    {


        public JavascriptTag(string path, IHtmlHelper helper, string version = null) : base(path, helper, "js", version)
        {

         
        }

        public override string ToString()
        {
             return $"<script src=\"/{this.Url}?BuildVersion={this.Version}\"></script>";
        }
    }
}
