using Microsoft.Azure.DigitalTwins.Parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace IoTModels.Resolvers
{
    class modelindexitem
    {
        public string path { get; set; }
        public string[] depends { get; set; }
    }

    public static class PublicRepoResolver
    {
        const string modelRepoUrl = "https://iotmodels.github.io/registry/";
        static WebClient wc = new WebClient();
        static IDictionary<string, modelindexitem> index;

        static PublicRepoResolver()
        {
            Console.Write("Downloading Index.. ");
            var modelIndexJson = wc.DownloadString(modelRepoUrl + "model-index.json");
            index = JsonConvert.DeserializeObject<IDictionary<string, modelindexitem>>(modelIndexJson);
            Console.WriteLine(".. Loaded !!");
        }

        static public async Task<IEnumerable<string>> DtmiResolver(IReadOnlyCollection<Dtmi> dtmis)
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
