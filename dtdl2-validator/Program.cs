using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace dtdl2_validator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource(5000);

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();
            
            ILogger logger = LoggerFactory.Create(builder =>
                builder
                .AddConfiguration(config.GetSection("Logging"))
                .AddDebug()
                .AddConsole()
            ).CreateLogger<ModelValidationService>();

            var validator = new ModelValidationService(config, logger);
            await validator.ExecuteAsync(cancellationTokenSource.Token);
            
        }
    }
}
