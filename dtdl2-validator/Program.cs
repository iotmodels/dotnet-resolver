using IoTModels.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;

namespace dtdl2_validator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource(5000);
            var host = Host.CreateDefaultBuilder(args)
              .ConfigureServices((hostContext, services) =>
              {
                  services.AddHostedService<ModelValidationService>();
              });
            await host.RunConsoleAsync(cancellationTokenSource.Token).ConfigureAwait(true);
        }
    }
}
