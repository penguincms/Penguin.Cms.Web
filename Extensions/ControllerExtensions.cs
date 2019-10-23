using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Penguin.Cms.Web.Extensions
{
    public static class ControllerExtensions
    {
        public static void AddMessage(this Controller c, string Message)
        {
            List<string> ExistingMessages = (c.TempData["Messages"] as List<string>) ?? new List<string>();

            ExistingMessages.Add(Message);

            c.TempData["Messages"] = ExistingMessages;
        }
    }
}