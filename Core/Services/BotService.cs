using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Services
{
    public class BotService : BackgroundService
    {
        private readonly PairDetector _pairDetector;
        private readonly ILogger<BotService> _logger;

        public BotService(PairDetector pairDetector, ILogger<BotService> logger)
        {
            _pairDetector = pairDetector;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bot service starting...");

            try
            {
                await _pairDetector.StartListeningAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in bot service");
                throw;
            }
        }
    }
}
