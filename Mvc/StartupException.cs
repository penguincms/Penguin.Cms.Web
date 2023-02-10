using System;

namespace Penguin.Cms.Web.Mvc
{
    public class StartupException
    {
        public Exception Exception { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        public StartupException(Exception exception)
        {
            Exception = exception;
        }
    }
}