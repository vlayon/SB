using Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Crypto Snipping Bot Starting ===\n");

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    // Bind configuration
                    var botConfig = context.Configuration.GetSection("BotConfig").Get<BotConfiguration>();
                    services.AddSingleton(botConfig);

                    // Register services
                    services.AddSingleton<PairDetector>();
                    services.AddSingleton<TokenTrader>();
                    services.AddSingleton<PriceMonitor>();
                    services.AddSingleton<HoneypotChecker>();
                    services.AddHostedService<BotService>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddFile("logs/snipbot-{Date}.log");
                })
                .Build();

            await host.RunAsync();
        }
    }
}
