using Penguin.Files.Services;
using System.IO;

namespace Penguin.Cms.Web.Extensions
{
    public static class FileServiceExtensions
    {
        /// <summary>
        /// Checks if a file exists on disk. This is here because IO on the disk is VERY slow
        /// </summary>
        /// <param name="fileService"></param>
        /// <param name="Uri"></param>
        /// <returns></returns>
        public static bool ContentExists(this FileService fileService, string Uri)
        {
            return fileService is null
                ? throw new System.ArgumentNullException(nameof(fileService))
                : Uri is null
                ? throw new System.ArgumentNullException(nameof(Uri))
                : fileService.Exists(Path.Combine("~/wwwroot", TrimTilde(Uri)));
        }

        private static string TrimTilde(string uri)
        {
            return !uri.StartsWith("~", System.StringComparison.OrdinalIgnoreCase) ? uri : uri[2..];
        }
    }
}