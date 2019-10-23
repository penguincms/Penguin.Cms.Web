﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Penguin.Cms.Web.Mvc.Exceptions;
using Penguin.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Penguin.Cms.Web.Mvc
{
    [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
    public class ControllerFactory : IControllerFactory
    {
        public ControllerFactory()
        {
        }

        public static Type GetControllerType(string controllerName, string? area = "")
        {
            area ??= "";

            List<Type> MatchingControllers = new List<Type>();

            foreach (Type t in TypeFactory.GetDerivedTypes(typeof(Controller)))
            {
                AreaAttribute? a = t.GetCustomAttribute<AreaAttribute>();

                bool match = true;

                match = match && string.Equals(controllerName + "Controller", t.Name, StringComparison.OrdinalIgnoreCase);

                match = match && ((string.IsNullOrWhiteSpace(area) && a is null) || string.Equals(a?.RouteValue, area, StringComparison.OrdinalIgnoreCase));

                if (match)
                {
                    MatchingControllers.Add(t);
                }
            }

            foreach (Type t in MatchingControllers.ToList())
            {
                MatchingControllers.AddRange(TypeFactory.GetDerivedTypes(t));
            }

            Type toReturn = TypeFactory.GetMostDerivedType(MatchingControllers, typeof(Controller));

            return toReturn;
        }

        public object CreateController(ControllerContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            string? controllerName = context.RouteData.Values["controller"].ToString();

            if (controllerName is null)
            {
                throw new Exception("Context route data does not contain definition for controller");
            }

            Type ControllerType = GetControllerType(controllerName, context.RouteData.Values["area"]?.ToString() ?? "");

            if (ControllerType == null)
            {
                throw new ControllerNotFoundException("No Controller found that matches the type: " + controllerName + "Controller");
            }

            Controller controller = (Controller)context.HttpContext.RequestServices.GetService(ControllerType);

            return controller;
        }

        public void ReleaseController(ControllerContext context, object controller)
        {
            throw new NotImplementedException();
        }
    }
}