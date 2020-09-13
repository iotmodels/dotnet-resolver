using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace IoTModels.Resolvers
{
    public class LocalFSResolver : IResolver
    {
        string baseFolder;
        ILogger logger;
        public LocalFSResolver(IConfiguration config, ILogger log)
        {
            logger = log;
            baseFolder = config.GetValue<string>("baseFolder");
            if (string.IsNullOrEmpty(baseFolder))
            {
                baseFolder = ".";
            }
        }

        public async Task<IEnumerable<string>> DtmiResolver(IReadOnlyCollection<Dtmi> dtmis)
        {
            List<string> resolvedModels = new List<string>();
            foreach (var dtmi in dtmis)
            {
                logger.LogInformation($"Resolving {dtmi.AbsoluteUri}");
                var path = DtmiConvention.Dtmi2Path(dtmi.AbsoluteUri);
                DirectoryInfo di = new DirectoryInfo(baseFolder);
                string uri = di.FullName;
                foreach(var f in path.Split('/'))
                {
                    uri = Path.Combine(uri, f);
                }
                logger.LogTrace("Reading: " + uri);
                resolvedModels.Add(File.ReadAllText(uri));
                logger.LogTrace("OK:" + uri);
            }
            return await Task.FromResult(resolvedModels);
        }

        async Task<string> Get(string url)
        {
            logger.LogInformation("GET: " + url);
            using (var http = new HttpClient())
            {
                var data = await http.GetStringAsync(url);
                return data;
            }
        }
    }
}
