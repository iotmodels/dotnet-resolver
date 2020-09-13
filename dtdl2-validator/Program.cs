using IoTModels.Resolvers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;

namespace dtdl2_validator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var loggerFactory = LoggerFactory.Create(builder => 
            {
                    builder.AddConsole();
            });  

            var cancellationTokenSource = new CancellationTokenSource(5000);
           ILogger<ModelValidationService> logger = loggerFactory.CreateLogger<ModelValidationService>();
           var validator = new ModelValidationService(config, logger);
           await validator.ExecuteAsync(cancellationTokenSource.Token);
        }
    }
}
