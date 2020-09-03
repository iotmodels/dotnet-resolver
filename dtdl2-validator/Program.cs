using IoTModels.Resolvers;
using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace dtdl2_validator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            (string input, string resolver) = ValidateInput(args);
            PrintHeader(input, resolver);

            ModelParser parser = new ModelParser();
            if (resolver == "local")
            {
                parser.DtmiResolver = LocalFolderResolver.DtmiResolver;
            }
            else
            {
                parser.DtmiResolver = PublicRepoResolver.DtmiResolver;
            }
            try
            {
                var parserResult = await parser.ParseAsync(new string[] { File.ReadAllText(input) });
                Console.WriteLine("Resolution completed\n\n");
                foreach (var item in parserResult.Values
                    .Where(v => v.EntityKind == DTEntityKind.Interface 
                             || v.EntityKind == DTEntityKind.Component))
                {
                    Console.WriteLine(item.Id);
                }
                Console.WriteLine($"\nValidation Passed: {input}");
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

        private static void PrintHeader(string input, string resolver)
        {
            Console.WriteLine("\n-----------------------------------");
            Console.WriteLine($"dtdl2-validator {input} {resolver}");
            Console.WriteLine($"version: {ThisAssemblyVersion} using parser: {ThisParserVersion}");
            Console.WriteLine("-----------------------------------");
        }

        static (string, string) ValidateInput(string[] args)
        {
            string input;
            string resolver;
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: dtdl2-validator <dtdlFile.json> <resolver?local|public>");
                System.Environment.Exit(1);
            }
            input = args[0];
            if (args.Length > 1)
            {
                if (!File.Exists(input))
                {
                    Console.WriteLine($"File '{input}' not found");
                    System.Environment.Exit(1);
                }
                if (args[1]=="local")
                {
                    resolver = "local";
                }
                else
                {
                    Console.WriteLine("Unknown resolver:" + args[1]);
                    resolver = "public";
                }
            }
            else
            {
                resolver = "public";
            }
            return (input, resolver);
        }

        static string ThisAssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        static string ThisParserVersion => typeof(ModelParser).Assembly.GetName().Version.ToString();
    }
}
