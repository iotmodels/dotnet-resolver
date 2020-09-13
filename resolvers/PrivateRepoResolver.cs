using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IoTModels.Resolvers
{
    public class PrivateRepoResolver : IResolver
    {
        string storageConnectionString;
        BlobContainerClient containerClient;
        ILogger logger;
        public PrivateRepoResolver(IConfiguration config, ILogger log)
        {
            this.logger = log;
            storageConnectionString = config.GetValue<string>("StorageConnectionString");
            if (string.IsNullOrEmpty(storageConnectionString))
            {
                Console.WriteLine("ERROR: PrivateRepos require credentials via config 'StorageConnectionString'");
                Environment.Exit(-1);
            }
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
            containerClient = blobServiceClient.GetBlobContainerClient("repo");
        }


        public async Task<IEnumerable<string>> DtmiResolver(IReadOnlyCollection<Dtmi> dtmis)
        {
            List<string> resolvedModels = new List<string>();
            foreach (var dtmi in dtmis)
            {
                
                    string path = DtmiConvention.Dtmi2Path(dtmi.AbsoluteUri);
                    var dlModel = await containerClient.GetBlobClient(path).DownloadAsync();
                    using (var sr = new StreamReader(dlModel.Value.Content))
                    {
                        resolvedModels.Add(sr.ReadToEnd());
                    }
                    logger.LogTrace("OK " + path);
                
            }
            return await Task.FromResult(resolvedModels);
        }
    }
}
