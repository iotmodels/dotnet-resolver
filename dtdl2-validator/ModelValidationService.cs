using AngleSharp.Html.Dom.Events;
using IoTModels.Resolvers;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dtdl2_validator
{
    class ModelValidationService : BackgroundService
    {
        readonly ILogger<ModelValidationService> log;
        readonly IConfiguration config;

        public ModelValidationService(ILogger<ModelValidationService> logger, IConfiguration configuration)
        {
            this.log = logger;
            this.config = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            (string input, string resolver) = ReadConfiguration(config);
            PrintHeader(input, resolver);
            int validationResult = await ValidateAsync(input, resolver);
            Environment.Exit(validationResult);
        }

        private static async Task<int> ValidateAsync(string input, string resolver)
        {
            ModelParser parser = new ModelParser();
            if (resolver != "none")
            {
                if (resolver == "local")
                {
                    parser.DtmiResolver = LocalFolderResolver.DtmiResolver;
                }
                else
                {
                    parser.DtmiResolver = PublicRepoResolver.DtmiResolver;
                }
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
                return 0;
            }
            catch (ResolutionException rex)
            {
                Console.WriteLine(rex.ToString());
                return -1;
            }
            catch (ParsingException pex)
            {
                Console.WriteLine(pex.ToString());
                return -2;
            }
        }


        private static void PrintHeader(string input, string resolver)
        {
            Console.WriteLine("\n-----------------------------------");
            Console.WriteLine($"dtdl2-validator {input} {resolver}");
            Console.WriteLine($"version: {ThisAssemblyVersion} using parser: {ThisParserVersion}");
            Console.WriteLine("-----------------------------------");
        }

        static (string, string) ReadConfiguration(IConfiguration config)
        {
            string input = config.GetValue<string>("f");
            string resolver = config.GetValue<string>("resolver"); ;

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Usage: dtdl2-validator /f=<dtdlFile.json> /resolver?=<local|public|none>");
                System.Environment.Exit(-1);
            }
            else
            {
                if (!File.Exists(input))
                {
                    Console.WriteLine($"File '{input}' not found");
                    System.Environment.Exit(-1);
                }
            }
            
            if (!string.IsNullOrEmpty(resolver))
            {
                if (resolver == "local")
                {
                    resolver = "local";
                }
                else
                {
                    Console.WriteLine("Unknown resolver:" + resolver);
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
