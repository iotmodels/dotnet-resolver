using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Configuration;
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
        class modelindexitem
        {
            public string path { get; set; }
            public string[] depends { get; set; }
        }

        string storageConnectionString;
        BlobContainerClient containerClient;
        IDictionary<string, modelindexitem> index;
        public PrivateRepoResolver(IConfiguration config)
        {
            storageConnectionString = config.GetValue<string>("StorageConnectionString");
            if (string.IsNullOrEmpty(storageConnectionString))
            {
                Console.WriteLine("ERROR: PrivateRepos require connectionstring 'StorageConnectionString'");
                Environment.Exit(-1);
            }
            LoadIndex(storageConnectionString).Wait();
        }

        private async Task LoadIndex(string storageConnectionString)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
            containerClient = blobServiceClient.GetBlobContainerClient("models");
            Console.WriteLine("Downloading index from blob storage");
            var dlIndex = await containerClient.GetBlobClient("model-index.json").DownloadAsync();
            using (var sr = new StreamReader(dlIndex.Value.Content))
            {
                index = JsonConvert.DeserializeObject<IDictionary<string, modelindexitem>>(sr.ReadToEnd());
                Console.WriteLine("Loaded index with : " + index.Count);
            }
        }

        public async Task<IEnumerable<string>> DtmiResolver(IReadOnlyCollection<Dtmi> dtmis)
        {
            List<string> resolvedModels = new List<string>();
            foreach (var dtmi in dtmis)
            {
                if (index.ContainsKey(dtmi.AbsoluteUri))
                {
                    string path = index[dtmi.AbsoluteUri].path;
                    Console.WriteLine("Downloading definition");
                    var dlModel = await containerClient.GetBlobClient(path).DownloadAsync();
                    using (var sr = new StreamReader(dlModel.Value.Content))
                    {
                        resolvedModels.Add(sr.ReadToEnd());
                    }
                    Console.WriteLine("OK " + path);
                }
            }
            return await Task.FromResult(resolvedModels);
        }
    }
}
