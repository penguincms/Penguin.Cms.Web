﻿using System;

namespace Penguin.Cms.Web.Mvc
{
    public class StartupException
    {
        public StartupException(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;
    }
}