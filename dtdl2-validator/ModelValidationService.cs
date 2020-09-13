
using IoTModels.Resolvers;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace dtdl2_validator
{
    class ModelValidationService : BackgroundService
    {
        readonly ILogger log;
        readonly IConfiguration config;
        private readonly IHostApplicationLifetime applicationLifetime;

        public ModelValidationService(IConfiguration configuration, ILogger<ModelValidationService> logger, IHostApplicationLifetime applicationLifetime)
        {
            this.log = logger;
            this.config = configuration;
            this.applicationLifetime = applicationLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            (string input, string resolverName) = ReadConfiguration(config);
            PrintHeader(input, resolverName);
            await ValidateAsync(input, resolverName);
            
        }

        private async Task ValidateAsync(string input, string resolverName)
        {
            ModelParser parser = new ModelParser();
            ConfigureResolver(parser, resolverName);

            parser.Options = new HashSet<ModelParsingOption>() { ModelParsingOption.StrictPartitionEnforcement };
            try
            {
                var parserResult = await parser.ParseAsync(new string[] { File.ReadAllText(input) });
                Console.WriteLine("Resolution completed\n\n");
                foreach (var item in parserResult.Values)
                {
                    this.log.LogTrace(item.Id.AbsoluteUri);
                }
                Console.WriteLine($"\nValidation Passed: {input}");
            } 
            catch (Exception ex)
            {
                log.LogError(ex, "DTDL Parser Exception");

                Environment.ExitCode = 1;
            }
            finally
            {
                applicationLifetime.StopApplication();
            }
        }

        private void ConfigureResolver(ModelParser parser, string resolverName)
        {
            if (resolverName != "none")
            {
                if (resolverName == "local")
                {
                    parser.DtmiResolver = new LocalFolderResolver(config, log).DtmiResolver;
                }
                else if (resolverName == "private")
                {
                    parser.DtmiResolver = new PrivateRepoResolver(config, log).DtmiResolver;
                }
                else
                {
                    parser.DtmiResolver = new PublicRepoResolver(config, log).DtmiResolver;
                }
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
                Environment.ExitCode = 2;
            }
            else
            {
                if (!File.Exists(input))
                {
                    Console.WriteLine($"File '{input}' not found");
                    Environment.ExitCode = 2;
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
