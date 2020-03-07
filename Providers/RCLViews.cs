using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Penguin.Extensions.Strings;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Penguin.Cms.Web.Providers
{
    public class RazorDirectoryContents : IDirectoryContents
    {
        public bool Exists { get; set; }
        public List<RazorFileInfo> RazorFileInfo { get; set; } = new List<RazorFileInfo>();

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            return this.RazorFileInfo.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.RazorFileInfo.GetEnumerator();
        }
    }

    public class RazorFileInfo : IFileInfo
    {
        public bool Exists { get; set; }

        public bool IsDirectory { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public long Length => 0;

        public string Name { get; set; }
        public string PhysicalPath { get; set; }

        public Stream CreateReadStream()
        {
            throw new NotImplementedException();
        }
    }

    public class RCLViews : IFileProvider
    {
        public static ConcurrentBag<RazorCompiledItemAttribute> RazorViews = new ConcurrentBag<RazorCompiledItemAttribute>();

        public static void Include(ApplicationPartManager apm)
        {
            string[] assemblyFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dll", SearchOption.AllDirectories);
            foreach (string assemblyFile in assemblyFiles)
            {
                try
                {
                    if (assemblyFile.EndsWith(".Views.dll"))
                    {
                        Assembly assembly = Assembly.LoadFrom(assemblyFile);
                        apm.ApplicationParts.Add(new CompiledRazorAssemblyPart(assembly));
                        foreach (RazorCompiledItemAttribute rcla in assembly.GetCustomAttributes<RazorCompiledItemAttribute>())
                        {
                            RazorViews.Add(rcla);
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            string PathToCheck = this.CleanPath(subpath, true);

            RazorDirectoryContents directoryContents = new RazorDirectoryContents();

            if (RazorViews.Any(r => r.Identifier.StartsWith(PathToCheck)))
            {
                directoryContents.Exists = true;

                foreach (RazorCompiledItemAttribute a in RazorViews.Where(rca => rca.Identifier.StartsWith(PathToCheck)))
                {
                    string LocalPath = a.Identifier.From(PathToCheck);
                    bool Directory = LocalPath.Contains("/");
                    string Name = LocalPath.To("/");

                    if (!directoryContents.Any(r => r.Name == Name))
                    {
                        RazorFileInfo fileInfo = new RazorFileInfo
                        {
                            Exists = true,
                            IsDirectory = Directory,
                            LastModified = DateTime.MinValue,
                            Name = Name,
                            PhysicalPath = ""
                        };

                        directoryContents.RazorFileInfo.Add(fileInfo);
                    }
                }
            }

            return directoryContents;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            string PathToCheck = this.CleanPath(subpath);

            RazorFileInfo fileInfo = new RazorFileInfo();

            RazorCompiledItemAttribute rca = RazorViews.FirstOrDefault(rcal => rcal.Identifier == PathToCheck);

            fileInfo.Exists = rca != null;

            if (rca != null)
            {
                string LocalPath = rca.Identifier.From(PathToCheck);
                bool Directory = LocalPath.Contains("/");
                string Name = LocalPath.To("/");

                fileInfo.IsDirectory = Directory;
                fileInfo.LastModified = DateTime.MinValue;
                fileInfo.Name = Name;
                fileInfo.PhysicalPath = "";
            }

            return fileInfo;
        }

        public IChangeToken Watch(string filter)
        {
            throw new NotImplementedException();
        }

        private string CleanPath(string toClean, bool Directory = false)
        {
            string pathToCheck = toClean;

            if (!pathToCheck.EndsWith("/") && Directory)
            {
                pathToCheck = $"{pathToCheck}/";
            }

            if (pathToCheck.StartsWith("~"))
            {
                pathToCheck = pathToCheck.Substring(1);
            }

            if (!pathToCheck.StartsWith("/"))
            {
                pathToCheck = $"/{pathToCheck}";
            }

            return pathToCheck;
        }
    }
}