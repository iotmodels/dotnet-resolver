using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IoTModels.Resolvers
{
    public class LocalFolderResolver : IResolver
    {
        const string baseFolder = "models";
        IDictionary<string, string> index = new Dictionary<string, string>();
        public LocalFolderResolver()
        {
            if (!Directory.Exists(baseFolder))
            {
                Console.WriteLine($"ERROR. BaseFolder '{baseFolder}' not found.");
            }
            TraverseDir(new DirectoryInfo(baseFolder));
        }

        void TraverseDir(DirectoryInfo di)
        {
            foreach (var subfolder in di.EnumerateDirectories())
            {
                foreach (var file in subfolder.GetFiles("*.json"))
                {
                    (string dtmi, string content) = LightParser.SemiParse(file.FullName);
                    index.Add(dtmi, content);
                    Console.WriteLine($"Found {dtmi} in local folder {subfolder.FullName}");
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
    }
}
