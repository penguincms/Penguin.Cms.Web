using System;

namespace Penguin.Cms.Web.Mvc.Exceptions
{
    public class ControllerNotFoundException : Exception
    {
        public ControllerNotFoundException(string Message) : base(Message)
        {
        }

        public ControllerNotFoundException()
        {
        }

        public ControllerNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}