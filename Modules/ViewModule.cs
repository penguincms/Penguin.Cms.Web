using Penguin.Cms.Abstractions.Interfaces;

namespace Penguin.Cms.Web.Modules
{
    public class ViewModule : IViewModule<object>
    {
        public string Id => this.Name.Replace(" ", "_");
        public object Model { get; }

        public string Name { get; }
        public string ViewPath { get; }

        public ViewModule(string viewPath, object model, string name)
        {
            this.Model = model;
            this.ViewPath = viewPath;
            this.Name = name;
        }
    }
}