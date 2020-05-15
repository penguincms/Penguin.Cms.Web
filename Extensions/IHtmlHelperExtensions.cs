using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Penguin.Cms.Web.Mvc;
using Penguin.Files.Services;
using Penguin.Reflection.Serialization.Abstractions.Interfaces;
using Penguin.Reflection.Serialization.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Penguin.Cms.Web.Extensions
{
    public static class IHtmlHelperExtensions
    {
        public const string RENDERS = "ResourcesToRender";

        public const string SCRIPTS = "ScriptsToRender";

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static Func<IHtmlContent> Helper(
          this RazorPageBase page,
          Func<Func<object, IHtmlContent>> helper
        )
        {
            return () => helper()(null!);
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static Func<T1, IHtmlContent> Helper<T1>(
          this RazorPageBase page,
          Func<T1, Func<object, IHtmlContent>> helper
        )
        {
            return p1 => helper(p1)(null!);
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static Func<T1, T2, IHtmlContent> Helper<T1, T2>(
          this RazorPageBase page,
          Func<T1, T2, Func<object, IHtmlContent>> helper
        )
        {
            return (p1, p2) => helper(p1, p2)(null!);
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static Func<T1, T2, T3, IHtmlContent> Helper<T1, T2, T3>(
          this RazorPageBase page,
          Func<T1, T2, T3, Func<object, IHtmlContent>> helper

        )
        {
            return (p1, p2, p3) => helper(p1, p2, p3)(null!);
        }

        public static void AddResource(this IHtmlHelper helper, string resourceString)
        {
            if (helper is null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            if (!(helper.ViewContext.HttpContext.Items[RENDERS] is List<string> resourceManager))
            {
                resourceManager = new List<string>();
                helper.ViewContext.HttpContext.Items.Add(RENDERS, resourceManager);
            }

            resourceManager.Add(resourceString);
        }

        public static IHtmlContent Action(this IHtmlHelper helper, string action, object[]? parameters = null)
        {
            if (helper is null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            string controller = (string)helper.ViewContext.RouteData.Values["controller"];

            return Action(helper, action, controller, parameters);
        }

        public static IHtmlContent Action(this IHtmlHelper helper, string action, string controller, object?[]? parameters = null)
        {
            if (helper is null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            string area = (string)helper.ViewContext.RouteData.Values["area"];

            return helper.Action(action, controller, area, parameters);
        }

        public static HtmlString RenderIncludes(this IHtmlHelper helper)
        {
            if (helper is null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            string output = string.Empty;

            if (helper.ViewContext.HttpContext.Items[RENDERS] is List<string> resourceManager)
            {
                foreach (string s in resourceManager)
                {
                    output = s + "\r\n\t" + output;
                }
            }

            //output = helper.PartialToString("FrameworkIncludeHtml") + output;

            return new HtmlString(output);
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static HtmlString MaterialIcon(this IHtmlHelper helper, string name)
        {
            return new HtmlString($"<i class=\"material-icons md-24\" icon-name=\"{name}\">{name}</i>");
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static HtmlString MaterialIcon(this IHtmlHelper helper, string name, Dictionary<string, string> attributes)
        {
            if (attributes is null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            string attributeStrings = string.Empty;

            foreach (KeyValuePair<string, string> kvp in attributes)
            {
                attributeStrings += $" {kvp.Key}=\"{kvp.Value}\"";
            }

            return new HtmlString($"<i class=\"material-icons md-24\" {attributeStrings} icon-name=\"{name}\">{name}</i>");
        }

        public static IHtmlContent MetaRoute(this IHtmlHelper helper, Dictionary<string, object> routeValues, IMetaObject toRender)
        {
            if (routeValues is null)
            {
                throw new ArgumentNullException(nameof(routeValues));
            }

            if (toRender is null)
            {
                throw new ArgumentNullException(nameof(toRender));
            }

            string ControllerName = routeValues["controller"]?.ToString() ?? string.Empty;
            string ActionName = routeValues["action"]?.ToString() ?? string.Empty;
            string AreaName = routeValues["area"]?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(ControllerName))
            {
                throw new ArgumentException("Controller name must be specified", nameof(routeValues));
            }

            if (string.IsNullOrWhiteSpace(ActionName))
            {
                throw new ArgumentException("Action name must be specified", nameof(routeValues));
            }

            Type ControllerType = ControllerFactory.GetControllerType(ControllerName, routeValues["area"].ToString());

            if (ControllerType is null)
            {
                throw new Exception($"Controller type not found for {ControllerName}");
            }

            MethodInfo? m = ControllerType.GetMethod(ActionName);

            if (m is null)
            {
                return new HtmlString($"Could not find method {ActionName} on controller {ControllerName}");
            }

            ParameterInfo[] MethodParameters = m.GetParameters();

            object? model = null;

            if (MethodParameters.Any())
            {
                Type TargetParameter = MethodParameters.First().ParameterType;

                //If the action we're sending this too doesn't accept a MetaObject then it
                //Likely accepts the real value type the object was created from so we cast it back
                //If that doesn't work then theres something wrong with the way everything is tagged
                if (!TargetParameter.IsAssignableFrom(toRender.GetType()))
                {
                    model = toRender.GetValue(TargetParameter);
                }
                else
                {
                    model = toRender;
                }
            }

            return helper.Action(ActionName, ControllerName, AreaName, new object?[] { model });
        }

        public static IHtmlContent Action(this IHtmlHelper helper, string action, string controller, string area, params object?[]? parameters)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (controller is null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            Task<IHtmlContent> task = helper.RenderActionAsync(action, controller, area, parameters);

            return task.Result;
        }

        // etc. for as high as you want to take the # of parameters

        public static async Task<IHtmlContent> RenderActionAsync(this IHtmlHelper helper, string action, string controller, string area, object?[]? parameters = null)
        {
            if (helper is null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            return await helper.ViewContext.HttpContext.RenderActionAsync(action, controller, area, parameters, helper.ViewContext).ConfigureAwait(true);
        }

        public static async Task<IHtmlContent> RenderActionAsync(this HttpContext context, string action, string controller, string area, object?[]? parameters = null, ViewContext? existingViewContext = null)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (controller is null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            HttpContext currentHttpContext = existingViewContext?.HttpContext ?? context;
            parameters ??= Array.Empty<object>();
            // fetching required services for invocation

            IHttpContextFactory httpContextFactory = GetServiceOrFail<IHttpContextFactory>(currentHttpContext);

            HttpContext newHttpContext = httpContextFactory.Create(currentHttpContext.Features);

            IActionInvokerFactory actionInvokerFactory = GetServiceOrFail<IActionInvokerFactory>(newHttpContext);
            IActionDescriptorCollectionProvider actionSelector = GetServiceOrFail<IActionDescriptorCollectionProvider>(newHttpContext);

            // creating new action invocation context
            RouteData routeData = new RouteData();

            RouteValueDictionary routeValues = new RouteValueDictionary(new { area, controller, action });

            IRazorViewEngine ViewEngine = GetServiceOrFail<IRazorViewEngine>(newHttpContext);

            newHttpContext.Response.Body = new MemoryStream();

            routeData.PushState(null, routeValues, null);

            Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor actionDescriptor = actionSelector.ActionDescriptors.Items.First(i => i.RouteValues["Controller"] == controller && i.RouteValues["Action"] == action);

            ActionContext actionContext = new ActionContext(newHttpContext, routeData, actionDescriptor);

            //We're not invoking this using an invoker beacause it doesn't support as many object types as parameters.
            IControllerFactory cf = GetServiceOrFail<IControllerFactory>(newHttpContext);

            //build the controller
            Controller c = (Controller)cf.CreateController(new ControllerContext(actionContext));

            //call the action to build the information required to fill the view
            ViewResult? viewResult = c.GetType()?.GetMethod(action)?.Invoke(c, parameters) as ViewResult;

            //Find the view the action says it uses
            ViewEngineResult result = ViewEngine.FindView(actionContext, viewResult?.ViewName ?? action, false);

            string htmlContent = string.Empty;

            using (StringWriter output = new StringWriter())
            {
                HtmlHelperOptions options;

                if (existingViewContext != null)
                {
                    options = new HtmlHelperOptions()
                    {
                        ClientValidationEnabled = existingViewContext.ClientValidationEnabled,
                        Html5DateRenderingMode = existingViewContext.Html5DateRenderingMode,
                        ValidationSummaryMessageElement = existingViewContext.ValidationSummaryMessageElement,
                        ValidationMessageElement = existingViewContext.ValidationMessageElement
                    };
                }
                else
                {
                    options = new HtmlHelperOptions()
                    {
                        ClientValidationEnabled = true,
                        Html5DateRenderingMode = Html5DateRenderingMode.CurrentCulture,
                        ValidationSummaryMessageElement = "validation-summary-errors",
                        ValidationMessageElement = "validation-summary-messages"
                    };
                }

                ViewContext viewcontext = new ViewContext(actionContext, result.View, viewResult?.ViewData, viewResult?.TempData, output, options);

                await result.View.RenderAsync(viewcontext).ConfigureAwait(false);

                htmlContent = output.ToString();
            }

            return new HtmlString(htmlContent);
        }

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        private static TService GetServiceOrFail<TService>(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            object service = httpContext.RequestServices.GetService(typeof(TService));

            if (service == null)
            {
                throw new InvalidOperationException($"Could not locate service: {nameof(TService)}");
            }

            return (TService)service;
        }

        /// <summary>
        /// Renders the partial to a string
        /// </summary>
        /// <param name="helper">The current HtmlHelper from the calling context</param>
        /// <param name="partialName">The name of the view to render</param>
        /// <returns>A string representation of the body of the rendered partial</returns>
        public static string PartialToString(this IHtmlHelper helper, string partialName)
        {
            using StringWriter writer = new System.IO.StringWriter();
#pragma warning disable MVC1000 // Use of IHtmlHelper.{0} should be avoided.
            helper.Partial(partialName).WriteTo(writer, HtmlEncoder.Default);
#pragma warning restore MVC1000 // Use of IHtmlHelper.{0} should be avoided.
            return writer.ToString();
        }

        /// <summary>
        /// Adds the given filenames to the back end list of CSS files to find and include in the layout
        /// </summary>
        /// <param name="helper">The current HtmlHelper from the calling context</param>
        /// <param name="fileNames">A list of the filenames to add to the back end list of CSS files</param>
        public static void IncludeCSS(this IHtmlHelper helper, params string[] fileNames)
        {
            string version = DateTime.Now.ToString("yyyyMMddhhmm", CultureInfo.CurrentCulture);

            foreach (string resourceName in fileNames)
            {
                string file = resourceName;

                if (!file.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    file = $"css/{file}";
                }
                else
                {
                    file = resourceName.Substring(1);
                }

                string cssUrl = $"{file}.css";
                string cssExtensionUrl = $"{file}.extension.css";

                string linkString = "<link href=\"{0}?BuildVersion={1}\" rel=\"stylesheet\" />";

                void addLinkString(string s)
                {
                    helper.AddResource(string.Format(CultureInfo.CurrentCulture, linkString, "/" + s, version));
                }

                addLinkString(cssUrl);

                if (helper.UrlExists(cssExtensionUrl))
                {
                    addLinkString(cssExtensionUrl);
                }
            }
        }

        /// <summary>
        /// Returns a registered instance of a FileService 
        /// </summary>
        /// <param name="helper">The current HtmlHelper from the calling context</param>
        /// <returns>A registered instance of a FileService </returns>
        public static FileService GetFileService(this IHtmlHelper helper)
        {
            if (helper is null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            return helper.ViewContext.HttpContext.RequestServices.GetService<FileService>();
        }

        private const string URL_EMPTY_MESSAGE = "Url can not be null or whitespace";

        /// <summary>
        /// Checks if the current path exists under the wwwroot folder, using the registered FileService
        /// </summary>
        /// <param name="helper">The current HtmlHelper from the calling context</param>
        /// <param name="url">The path to check, relative to the wwwroot folder</param>
        /// <returns>True if the resource exists in the registered FileService</returns>
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
        public static bool UrlExists(this IHtmlHelper helper, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException(URL_EMPTY_MESSAGE, nameof(url));
            }

            return helper.GetFileService().Exists(Path.Combine("wwwroot", url));
        }

        public static void IncludeJS(this IHtmlHelper helper, params string[] fileNames)
        {
            string version = DateTime.Now.ToString("yyyyMMddhhmm", CultureInfo.CurrentCulture);
            foreach (string resourceName in fileNames.Reverse())
            {
                string file = resourceName;

                if (!file.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    file = $"js/{file}";
                }
                else
                {
                    file = resourceName.Substring(1);
                }

                string jsUrl = $"{file}.js";
                string jsExtensionUrl = $"{file}.extension.js";

                string linkString = "<script src=\"{0}?BuildVersion={1}\"></script>";

                void addLinkString(string s)
                {
                    helper.AddResource(string.Format(CultureInfo.CurrentCulture, linkString, "/" + s, version));
                }

                addLinkString(jsUrl);

                if (helper.UrlExists(jsExtensionUrl))
                {
                    addLinkString(jsExtensionUrl);
                }
            }
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
        public static HtmlString Attribute(this IHtmlHelper helper, string name, string value, bool render)
        {
            string output = string.Empty;

            if (render)
            {
                output = $"{name}=\"{value}\"";
            }

            return new HtmlString(output);
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
        public static HtmlString Attribute(this IHtmlHelper helper, string name, bool render)
        {
            string output = string.Empty;

            if (render)
            {
                output = name;
            }

            return new HtmlString(output);
        }
    }
}