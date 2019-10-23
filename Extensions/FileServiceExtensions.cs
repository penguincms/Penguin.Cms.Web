using Penguin.Files.Services;
using System.IO;

namespace Penguin.Cms.Web.Extensions
{
    public static class FileServiceExtensions
    {
        /// <summary>
        /// Checks if a file exists on disk. This is here because IO on the disk is VERY slow
        /// </summary>
        /// <param name="Uri"></param>
        /// <returns></returns>
        public static bool ContentExists(this FileService fileService, string Uri)
        {
            return fileService.Exists(Path.Combine("~/wwwroot", TrimTilde(Uri)));
        }

        private static string TrimTilde(string uri)
        {
            if (!uri.StartsWith("~"))
            {
                return uri;
            }
            else
            {
                return uri.Substring(2);
            }
        }
    }
}