using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;

namespace Penguin.Cms.Web.Mvc.Razor
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (viewLocations is null)
            {
                throw new System.ArgumentNullException(nameof(viewLocations));
            }

            //Replace folder view with CustomViews
            //return viewLocations.Select(f => f.Replace("/Views/", "/CustomViews/"));

            List<string> expandedViewLocations = new();

            foreach (string viewLocation in viewLocations)
            {
                expandedViewLocations.Add("/Client" + viewLocation);
                //expandedViewLocations.Add("/Client.Template" + viewLocation);
                expandedViewLocations.Add(viewLocation);
            }

            return expandedViewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        { }
    }
}