using IoTModels.Resolvers;
using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace dtdl2_validator
{ 
    class Program
    {
        static async Task Main(string[] args)
        {
            string input = ValidateInput(args);
            ModelParser parser = new ModelParser();
            parser.DtmiResolver = PublicRepoResolver.DtmiResolver;
            try
            {
                var parserResult = await parser.ParseAsync(new string[] { File.ReadAllText(input) });
                foreach (var item in parserResult.Values)
                {
                    Console.WriteLine(item.Id);
                }
            }
            catch (ResolutionException rex)
            {
                Console.WriteLine(rex.ToString());    
            }
            catch (ParsingException pex)
            {
                Console.WriteLine(pex.ToString());
            }
        }

        static string ValidateInput(string[] args)
        {
            string input;
            if (args.Length < 1)
            {
                Console.WriteLine("File to validate?");
                input = Console.ReadLine();
            }
            input = args[0];
            return input;
        }
    }
}
