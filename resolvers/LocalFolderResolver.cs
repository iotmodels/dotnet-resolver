using Microsoft.Azure.DigitalTwins.Parser;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IoTModels.Resolvers
{
    public static class LocalFolderResolver
    {
        const string baseFolder = "_models_";
        static IDictionary<string, string> index = new Dictionary<string, string>();
        static LocalFolderResolver()
        {
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }
            foreach (var file in Directory.GetFiles(baseFolder))
            {
                (string dtmi, string content) = LightParser.SemiParse(file);
                index.Add(dtmi, content);
            }

        }
        static public async Task<IEnumerable<string>> DtmiResolver(IReadOnlyCollection<Dtmi> dtmis)
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
