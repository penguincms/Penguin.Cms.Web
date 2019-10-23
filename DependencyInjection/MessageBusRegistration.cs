using Penguin.DependencyInjection.Abstractions.Enums;
using Penguin.DependencyInjection.Abstractions.Interfaces;
using Penguin.DependencyInjection.ServiceProviders;
using Penguin.Messaging.Abstractions.Interfaces;
using Penguin.Messaging.Core;
using Penguin.Reflection;
using System;
using DependencyEngine = Penguin.DependencyInjection.Engine;

namespace Penguin.Cms.Web.DependencyInjection
{
    public class MessageBusRegistration : IRegisterDependencies
    {
        public void RegisterDependencies(IServiceRegister serviceRegister)
        {
            serviceRegister.Register(typeof(MessageBus), typeof(MessageBus), ServiceLifetime.Transient);

            foreach(Type t in TypeFactory.GetAllImplementations(typeof(IMessageHandler<>)))
            {
                serviceRegister.Register(t, t, ServiceLifetime.Scoped);
            }
        }
    }
}