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

        private async Task<int> ValidateAsync(string input, string resolver)
        {
            ModelParser parser = new ModelParser();
            if (resolver != "none")
            {
                if (resolver == "local")
                {
                    parser.DtmiResolver = new LocalFolderResolver(config).DtmiResolver;
                }
                else if (resolver=="private")
                {
                    parser.DtmiResolver = new PrivateRepoResolver(config).DtmiResolver;
                }
                else 
                {
                    parser.DtmiResolver = new PublicRepoResolver(config).DtmiResolver;
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


        private void PrintHeader(string input, string resolver)
        {
            Console.WriteLine("\n-----------------------------------");
            Console.WriteLine($"dtdl2-validator {input} {resolver}");
            Console.WriteLine($"version: {ThisAssemblyVersion} using parser: {ThisParserVersion}");
            Console.WriteLine("-----------------------------------");
        }

        (string, string) ReadConfiguration(IConfiguration config)
        {
            string input = config.GetValue<string>("f");
            string resolver = config.GetValue<string>("resolver"); ;

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Usage: dtdl2-validator /f=<dtdlFile.json> /resolver?=<public|private|local|none>");
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
            
            if (string.IsNullOrEmpty(resolver))
            {
                resolver = "public";
            }
            
            return (input, resolver);
        }

        static string ThisAssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        static string ThisParserVersion => typeof(ModelParser).Assembly.GetName().Version.ToString();
    }
}
