﻿using Microsoft.AspNetCore.Mvc;
using Penguin.DependencyInjection.Abstractions.Enums;
using Penguin.DependencyInjection.Abstractions.Interfaces;
using Penguin.Reflection;
using System;

namespace Penguin.Cms.Web.DependencyInjection
{
    public class DependencyRegistrations : IRegisterDependencies
    {
        public void RegisterDependencies(IServiceRegister serviceRegister)
        {
            foreach (Type controllerType in TypeFactory.GetDerivedTypes(typeof(Controller)))
            {
                serviceRegister.Register(controllerType, controllerType, ServiceLifetime.Scoped);
            }
        }
    }
}