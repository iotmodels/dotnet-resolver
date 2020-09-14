using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace IoTModels.Resolvers
{
    public class PublicRepoResolver : IResolver
    {
        string modelRepoUrl;
        ILogger logger;
        public PublicRepoResolver(IConfiguration config, ILogger log)
        {
            logger = log;
            modelRepoUrl = config.GetValue<string>("modelRepoUrl");
            if (string.IsNullOrEmpty(modelRepoUrl))
            {
                log.LogInformation("PublicRepoResolver modelRepoUrl config not found, using default.");
                modelRepoUrl = "https://iotmodels.github.io/registry/";
            }
            log.LogInformation($"PublicRepoResolver configured with modelRepoUrl={modelRepoUrl}");
        }

        public async Task<IEnumerable<string>> DtmiResolver(IReadOnlyCollection<Dtmi> dtmis)
        {
            List<string> resolvedModels = new List<string>();
            foreach (var dtmi in dtmis)
            {
                logger.LogInformation($"Resolving {dtmi.AbsoluteUri}");
                var path = DtmiConvention.Dtmi2Path(dtmi.AbsoluteUri);
                string url = modelRepoUrl + path;
                logger.LogTrace("Request: " + url);
                resolvedModels.Add(await Get(url));
                logger.LogTrace("OK:" + url);
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
