using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IoTModels.Resolvers
{
    public class LocalFolderResolver : IResolver
    {
        IDictionary<string, string> index = new Dictionary<string, string>();
        ILogger logger;

        public LocalFolderResolver(IConfiguration config, ILogger log)
        {
            this.logger = log;
            var baseFolder = config.GetValue<string>("baseFolder");
            if (string.IsNullOrEmpty(baseFolder))
            {
                baseFolder = "dtmi";
            }
            if (!Directory.Exists(baseFolder))
            {
                Console.WriteLine($"ERROR. BaseFolder '{baseFolder}' not found.");
            }
            logger.LogInformation("Loading models from: " + baseFolder);
            if (Directory.Exists(baseFolder))
            { 
                TraverseDir(new DirectoryInfo(baseFolder));
            }
            
        }

        void TraverseDir(DirectoryInfo di)
        {
            logger.LogTrace($"LocalFolderResolver traversing {di.FullName}");
            foreach (var file in di.GetFiles("*.json"))
            {
                (string dtmi, string content) = SemiParse(file.FullName);
                // TODO: handle dependencies
                if (!index.ContainsKey(dtmi))
                {
                    index.Add(dtmi, content);
                    logger.LogTrace($"{dtmi} in {di.FullName}");
                }
            }

            foreach (var subfolder in di.EnumerateDirectories())
            {
                foreach (var file in subfolder.GetFiles("*.json"))
                {
                    (string dtmi, string content) = SemiParse(file.FullName);
                    // TODO: handle dependencies
                    index.Add(dtmi, content);
                    logger.LogTrace($"{dtmi} in {subfolder.FullName}");
                }
                TraverseDir(subfolder);
            }
        }

        public async Task<IEnumerable<string>> DtmiResolver(IReadOnlyCollection<Dtmi> dtmis)
        {
            List<string> resolvedModels = new List<string>();
            foreach (var dtmi in dtmis)
            {
                if (index.ContainsKey(dtmi.AbsoluteUri))
                {
                    resolvedModels.Add(index[dtmi.AbsoluteUri]);
                }
            }
            return await Task.FromResult(resolvedModels);
        }

        (string dtmi, string content) SemiParse(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }
            string content = File.ReadAllText(fileName);
            JObject json;
            try
            {
                json = JObject.Parse(content);
            }
            catch
            {
                throw new JsonReaderException("cant parse json");
            }


            var ctx = json.SelectToken("@context");
            if (ctx.Value<string>() != "dtmi:dtdl:context;2")
            {
                throw new ApplicationException("Not valid DTDL v2");
            }

            var id = json.SelectToken("@id");


            return (id.Value<string>(), content);
        }
    }
}
