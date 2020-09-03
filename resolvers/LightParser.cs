using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace IoTModels.Resolvers
{
    public class LightParser
    {
        public static (string dtmi, string content) SemiParse(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }
            string content = File.ReadAllText(fileName);
            JObject json;
            try
            {
                json = JObject.Parse(content);
            }
            catch
            {
                throw new JsonReaderException("cant parse json");
            }


            var ctx = json.SelectToken("@context");
            if (ctx.Value<string>() != "dtmi:dtdl:context;2")
            {
                throw new ApplicationException("Not valid DTDL v2");
            }

            var id = json.SelectToken("@id");


            return (id.Value<string>(), content);
        }
    }
}
