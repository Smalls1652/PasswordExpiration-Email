using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Configuration;

namespace PasswordExpiration.AzFunction
{
    using Helpers.Services;

    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(
                    services =>
                    {
                        services.AddSingleton<IGraphClientService, GraphClientService>();
                        services.AddSingleton<IFunctionsConfigService, FunctionsConfigService>();
                    }
                )
                .Build();

            host.Run();
        }
    }
}