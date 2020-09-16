
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
            logger = log;
            var configRepolist = config.GetValue<string>("repoList");
            if (string.IsNullOrEmpty(configRepolist))
            {
                logger.LogWarning("Config 'repoList' not found, using default public repo.");
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
            List<string> knownDmtis = new List<string>();
            foreach (var dtmi in dtmis)
            {
                if (!knownDmtis.Contains(dtmi.AbsoluteUri))
                {
                    foreach (string repo in repoList)
                    {
                        logger.LogInformation($"Resolving {dtmi.AbsoluteUri} in repo {repo}.");
                        string url = repo + DtmiConvention.Dtmi2Path(dtmi.AbsoluteUri);

                        if (await http.Head(url, logger))
                        {
                            resolvedModels.Add(await http.Get(url, logger));
                            knownDmtis.Add(dtmi.AbsoluteUri);
                            logger.LogTrace("Found:" + url);
                            break;
                        }
                        else
                        {
                            logger.LogTrace("Not Found:" + url);
                        }
                            

                    }
                }
            }
            return await Task.FromResult(resolvedModels);
        }
    }
}
