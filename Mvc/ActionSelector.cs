using Loxifi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Penguin.Reflection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Cms.Web.Mvc
{
	internal class ActionSelector : IActionSelector
	{
		private readonly IActionSelector? _defaultActionSelector;

		public ActionSelector(IActionSelector? defaultActionSelector)
		{
			this._defaultActionSelector = defaultActionSelector;
		}

		public ActionDescriptor SelectBestCandidate(RouteContext context, IReadOnlyList<ActionDescriptor> candidates)
		{
			List<ControllerActionDescriptor> controllerActionDescriptors = new List<ControllerActionDescriptor>();
			List<ActionDescriptor> trueCandidates = new List<ActionDescriptor>();

			foreach(ActionDescriptor candidate in candidates)
			{
				if(candidate is ControllerActionDescriptor controllerActionDescriptor)
				{
					controllerActionDescriptors.Add(controllerActionDescriptor);
				} else
				{
					trueCandidates.Add(candidate);
				}
			}

			if(controllerActionDescriptors.Count > 1)
			{
				Type mostDerived = TypeFactory.Default.GetMostDerivedType(controllerActionDescriptors.Select(c => c.ControllerTypeInfo.UnderlyingSystemType).Distinct(), typeof(Controller));
				List<ControllerActionDescriptor> matches = controllerActionDescriptors.Where(c => c.ControllerTypeInfo.UnderlyingSystemType == mostDerived).Distinct().ToList();
				trueCandidates.AddRange(matches);
			} else
			{
				trueCandidates.AddRange(controllerActionDescriptors);
			}

			return this._defaultActionSelector.SelectBestCandidate(context, trueCandidates);
		}

		public IReadOnlyList<ActionDescriptor> SelectCandidates(RouteContext context)
		{
			return this._defaultActionSelector.SelectCandidates(context);
		}
	}
}
