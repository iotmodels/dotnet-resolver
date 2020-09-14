using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
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
                log.LogInformation("PublicRepoResolver baseFolder config not found, using default.");
                baseFolder = ".";
            }
            log.LogInformation($"LocalFSResolver configured in '{baseFolder}'");
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
                if (File.Exists(uri)) 
                {
                    logger.LogTrace("Reading: " + uri);
                    resolvedModels.Add(File.ReadAllText(uri));
                    logger.LogTrace("OK:" + uri);
                }
                else
                {
                    logger.LogWarning($"{dtmi.AbsoluteUri} not found in {di.FullName}");
                }
            }
            return await Task.FromResult(resolvedModels);
        }
    }
}
