using IoTModels.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace dtdl2_validator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
              .ConfigureServices((hostContext, services) =>
                  services
                    .AddHostedService<ModelValidationService>()
                    .AddSingleton<LocalFolderResolver>());
            await host.RunConsoleAsync().ConfigureAwait(true);
            
        }
    }
}
