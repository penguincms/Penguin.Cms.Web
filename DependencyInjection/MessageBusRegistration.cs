using Penguin.DependencyInjection.Abstractions.Enums;
using Penguin.DependencyInjection.Abstractions.Interfaces;
using Penguin.Messaging.Abstractions.Interfaces;
using Penguin.Messaging.Core;
using Penguin.Reflection;
using System;

namespace Penguin.Cms.Web.DependencyInjection
{
    public class MessageBusRegistration : IRegisterDependencies
    {
        public void RegisterDependencies(IServiceRegister serviceRegister)
        {
            if (serviceRegister is null)
            {
                throw new ArgumentNullException(nameof(serviceRegister));
            }

            serviceRegister.Register(typeof(MessageBus), typeof(MessageBus), ServiceLifetime.Transient);

            foreach (Type t in TypeFactory.GetAllImplementations(typeof(IMessageHandler<>)))
            {
                serviceRegister.Register(t, t, ServiceLifetime.Scoped);
            }
        }
    }
}