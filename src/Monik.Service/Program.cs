using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Monik.Service
{
    class Program
    {
        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder()
                .UseSystemd()
                .UseWindowsService()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureHostConfiguration(config => { config.AddEnvironmentVariables("ASPNETCORE_"); })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .AddJsonFile("configs/appsettings.json",
                            optional: true, reloadOnChange: true)
                        .AddJsonFile($"configs/appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json",
                            optional: true, reloadOnChange: true);
                })
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    loggingBuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    loggingBuilder.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseConfiguration(
                            new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("configs/hosting.json", optional: true)
                                .Build()
                        )
                        .UseKestrel(serverOptions => { serverOptions.AllowSynchronousIO = true; })
                        .UseStartup<Startup>();
                });
    }
}