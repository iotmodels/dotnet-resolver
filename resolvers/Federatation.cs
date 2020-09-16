
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTModels.Resolvers
{
    public class Federatation
    {
        ILogger logger;
        List<string> repoList = new List<string>();
        public Federatation(IConfiguration config, ILogger log)
        {
            var configRepolist = config.GetValue<string>("repoList");
            if (string.IsNullOrEmpty(configRepolist))
            {
                log.LogWarning("Config 'repoList' not found, using default public repo.");
                repoList.Add("https://iotmodels.github.io/registry");
            }
            else
            {
                repoList = configRepolist.Split(';').ToList<string>();
            }
        }
        public async Task<IEnumerable<string>> DtmiResolver(IReadOnlyCollection<Dtmi> dtmis)
        {
            List<string> resolvedModels = new List<string>();
            foreach (var dtmi in dtmis)
            {
                logger.LogInformation($"Resolving {dtmi.AbsoluteUri}");
                var path = DtmiConvention.Dtmi2Path(dtmi.AbsoluteUri);

                foreach(string repo in repoList)
                {
                    string url = repo + path;
                    logger.LogTrace("Request: " + url);
                    if (await http.Head(url))
                    {
                        resolvedModels.Add(await http.Get(url));
                        logger.LogTrace("OK:" + url);
                        break;
                    }
                }
            }
            return await Task.FromResult(resolvedModels);
        }
    }
}
