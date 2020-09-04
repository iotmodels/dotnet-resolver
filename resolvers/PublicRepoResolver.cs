using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace IoTModels.Resolvers
{
    public class PublicRepoResolver : IResolver
    {
        static WebClient wc = new WebClient();
        static IDictionary<string, modelindexitem> index;
        string modelRepoUrl;

        public PublicRepoResolver(IConfiguration config)
        {
            modelRepoUrl = config.GetValue<string>("modelRepoUrl");
            if (string.IsNullOrEmpty(modelRepoUrl))
            {
                modelRepoUrl = "https://iotmodels.github.io/registry/";
            }
            Console.Write("Downloading Index from " + modelRepoUrl);
            var modelIndexJson = wc.DownloadString(modelRepoUrl + "model-index.json");
            index = JsonConvert.DeserializeObject<IDictionary<string, modelindexitem>>(modelIndexJson);
            Console.WriteLine(".. Loaded !!");
        }

        public async Task<IEnumerable<string>> DtmiResolver(IReadOnlyCollection<Dtmi> dtmis)
        {
            List<string> resolvedModels = new List<string>();
            foreach (var dtmi in dtmis)
            {
                if (index.ContainsKey(dtmi.AbsoluteUri))
                {
                    string url = modelRepoUrl + index[dtmi.AbsoluteUri].path;
                    Console.WriteLine("Downloading definition");
                    resolvedModels.Add(wc.DownloadString(url));
                    Console.WriteLine("OK " + url);
                }
            }
            return await Task.FromResult(resolvedModels);
        }
    }
}
