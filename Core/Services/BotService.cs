using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Services
{
    public class BotService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BotService> _logger;

        public BotService(IServiceScopeFactory scopeFactory, ILogger<BotService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bot service starting...");

            try
            {
                await DoWorkAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Bot service cancellation requested.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in bot service");
                throw;
            }
        }

        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            // Create a scope and keep it alive for the duration of the listener.
            // This lets us resolve scoped services (PairDetector, repositories, DbContext, etc.)
            using var scope = _scopeFactory.CreateScope();
            var pairDetector = scope.ServiceProvider.GetRequiredService<PairDetector>();

            _logger.LogInformation("Starting PairDetector in scoped context.");
            await pairDetector.StartListeningAsync(cancellationToken);
            _logger.LogInformation("PairDetector stopped.");
        }
    }
}
