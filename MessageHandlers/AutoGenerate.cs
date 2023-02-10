using Penguin.Cms.Web.Constants.Strings;
using Penguin.Configuration.Abstractions.Interfaces;
using Penguin.Extensions.String;
using Penguin.Messaging.Abstractions.Interfaces;
using Penguin.Messaging.Application.Messages;
using Penguin.Persistence.Database;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Penguin.Cms.Web.MessageHandlers
{
    public class AutoGenerate : IMessageHandler<Startup>
    {
        public const string ROOTNS = ".Database.Generate.";
        public const string STOREDPROCEDUREFOLDER = "StoredProcedures";
        private const string UNSUPPORTED_SQL_TYPE_MESSAGE = "Unsupported SQL type";
        protected IProvideConfigurations ConfigurationService { get; set; }

        public AutoGenerate(IProvideConfigurations configurationService)
        {
            ConfigurationService = configurationService;
        }

        public void AcceptMessage(Startup message)
        {
            foreach (Assembly thisAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (thisAssembly is null || thisAssembly.IsDynamic)
                {
                    continue;
                }

                foreach (string Resource in thisAssembly.GetManifestResourceNames())
                {
                    if (Resource.Contains(ROOTNS, StringComparison.Ordinal))
                    {
                        if (Resource.From(ROOTNS).To(".") == STOREDPROCEDUREFOLDER)
                        {
                            string NameSpace = Resource.From(ROOTNS + STOREDPROCEDUREFOLDER + ".");

                            string Script = string.Empty;

                            using (Stream? stream = thisAssembly.GetManifestResourceStream(Resource))
                            {
                                if (stream is null)
                                {
                                    throw new NullReferenceException($"Manifest stream not found for resource {Resource}");
                                }

                                using StreamReader reader = new(stream);
                                Script = reader.ReadToEnd();
                            }

                            StoredProcedure thisProc = new(Script);

                            //Get the name after the room, trim the extention and then the proc name;

                            if (NameSpace.Count(c => c == '.') > 1)
                            {
                                NameSpace = NameSpace.ToLast(".").ToLast(".");

                                thisProc.RenameProcedure($"{NameSpace.Replace(".", "\\", StringComparison.OrdinalIgnoreCase)}\\{thisProc.Name}");
                            }

                            if (!thisProc.ConnectionStrings.Any())
                            {
                                thisProc.ConnectionStrings.Add(ConfigurationNames.DEFAULT_CONNECTION_STRING);
                            }

                            foreach (string ConnectionString in thisProc.ConnectionStrings)
                            {
                                string connectionString = ConfigurationService.GetConnectionString(ConnectionString);

                                //Cheap SDF hack. Should be fixed
                                if (!string.IsNullOrWhiteSpace(connectionString) && !connectionString.Contains(".sdf", StringComparison.OrdinalIgnoreCase))
                                {
                                    DatabaseInstance di = new(connectionString);

                                    di.ImportProcedure(thisProc);
                                }
                            }
                        }
                        else
                        {
                            throw new Exception(UNSUPPORTED_SQL_TYPE_MESSAGE);
                        }
                    }
                }
            }
        }
    }
}