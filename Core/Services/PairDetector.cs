using Core.Models;
using Data.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Net.Mail;

namespace Core.Services
{
    public class PairDetector
    {
        private readonly Web3 _web3;
        private readonly BotConfiguration _config;
        private readonly ILogger<PairDetector> _logger;
        private readonly TokenTrader _trader;
        private readonly HoneypotChecker _honeypotChecker;
        private readonly IPairRepository _pairRepository;

        // Uniswap V2 Factory ABI for PairCreated event
        private const string PAIR_CREATED_EVENT_ABI = @"[{
            'anonymous': false,
            'inputs': [
                {'indexed': true, 'name': 'token0', 'type': 'address'},
                {'indexed': true, 'name': 'token1', 'type': 'address'},
                {'indexed': false, 'name': 'pair', 'type': 'address'},
                {'indexed': false, 'name': '', 'type': 'uint256'}
            ],
            'name': 'PairCreated',
            'type': 'event'
        }]";

        public PairDetector(
            BotConfiguration config,
            ILogger<PairDetector> logger,
            TokenTrader trader,
            HoneypotChecker honeypotChecker,
            IPairRepository pairRepository)
        {
            _config = config;
            _logger = logger;
            _trader = trader;
            _honeypotChecker = honeypotChecker;
            _web3 = new Web3(_config.EthereumRpcUrl);
            _pairRepository = pairRepository;
        }

        public async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting to listen for new pairs on Uniswap V2 Factory: {Factory}",
                _config.UniswapV2FactoryAddress);

            await Verify();

            var contract = _web3.Eth.GetContract(PAIR_CREATED_EVENT_ABI, _config.UniswapV2FactoryAddress);
            var pairCreatedEvent = contract.GetEvent("PairCreated");

            // Get the current block number
            var currentBlock = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var fromBlock = new Nethereum.RPC.Eth.DTOs.BlockParameter(currentBlock);

            _logger.LogInformation("Successfully initialized. Listening for new pairs from block {Block}...", currentBlock.Value);

            // Poll for new events
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Get new block number
                    var latestBlock = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

                    // Create filter for the range
                    var filter = pairCreatedEvent.CreateFilterInput(fromBlock, new Nethereum.RPC.Eth.DTOs.BlockParameter(latestBlock));

                    // Get all events in this range
                    var eventLogs = await pairCreatedEvent.GetAllChangesAsync<PairCreatedEventDTO>(filter);

                    var dateTimeNow = DateTime.Now;
                    if (eventLogs.Count == 0) _logger.LogInformation($"[Date: {dateTimeNow.Day}.{dateTimeNow.Month}; Time:{dateTimeNow.Hour}:{dateTimeNow.Minute}] No events found.");

                    foreach (var eventLog in eventLogs)
                    {
                        await ProcessNewPairAsync(eventLog);
                    }

                    // Update the from block for next iteration
                    fromBlock = new Nethereum.RPC.Eth.DTOs.BlockParameter(new HexBigInteger(latestBlock.Value + 1));

                    // Wait before next poll
                    await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Pair detection cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("RPC connection issue: {Message}", ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message);
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }

        private async Task ProcessNewPairAsync(EventLog<PairCreatedEventDTO> eventLog)
        {
            // If configured to run detection-only (mock), skip real trading
            if (_config.MockExecuteRealTrade)
            {
                _logger.LogInformation("MockExecuteRealTrade is enabled — pair recorded but skipping real trade for {Pair}", eventLog.Event.Pair);
                return;
            }

            try
            {
                var token0 = eventLog.Event.Token0;
                var token1 = eventLog.Event.Token1;
                var pairAddress = eventLog.Event.Pair;

                _logger.LogInformation("🆕 New Pair Detected!");
                _logger.LogInformation("   Pair: {Pair}", pairAddress);
                _logger.LogInformation("   Token0: {Token0}", token0);
                _logger.LogInformation("   Token1: {Token1}", token1);

                await ProcessNewPairToDB(eventLog);

                // Check if one of the tokens is WETH
                bool isWethPair = token0.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase) ||
                                  token1.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase);

                if (_config.OnlyTradeWithWeth && !isWethPair)
                {
                    _logger.LogInformation("⏭️  Skipping - not a WETH pair");
                    return;
                }

                // Determine which token is the new token (not WETH)
                string targetToken = token0.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase)
                    ? token1 : token0;

                // Check if token is blacklisted
                if (_config.BlacklistedTokens.Any(bt => bt.Equals(targetToken, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogInformation("⛔ Token is blacklisted");
                    return;
                }

                // Run honeypot check if enabled
                if (_config.EnableHoneypotCheck)
                {
                    _logger.LogInformation("🔍 Running honeypot check...");
                    bool isSafe = await _honeypotChecker.CheckTokenSafetyAsync(targetToken, pairAddress);

                    if (!isSafe)
                    {
                        _logger.LogWarning("⚠️  Token failed safety check - skipping");
                        return;
                    }
                }

                // Create token info
                var tokenInfo = new TokenInfo
                {
                    Address = targetToken,
                    PairAddress = pairAddress,
                    Token0 = token0,
                    Token1 = token1,
                    DetectedAt = DateTime.UtcNow,
                    IsWethPair = isWethPair
                };

                // Execute trade
                _logger.LogInformation("✅ All checks passed - executing trade...");
                await _trader.ExecuteBuyOrderAsync(tokenInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing new pair");
            }
        }

        private async Task ProcessNewPairToDB(EventLog<PairCreatedEventDTO> eventLog)
        {
            // Check if already exists
            if (await _pairRepository.ExistsAsync(eventLog.Event.Pair))
            {
                _logger.LogInformation("    Pair with address {eventLog.Event.Pair} already exists", eventLog.Event.Pair);
                return;
            }

            // Record new pair
            var pair = await _pairRepository.CreatePairAsync(eventLog.Event.Token0, eventLog.Event.Token1, eventLog.Event.Pair);
            _logger.LogInformation("    Pair with address {eventLog.Event.Pair} is recorded", eventLog.Event.Pair);

        }

        async Task Verify()
        {
            var pk = _config.PrivateKey;
            var account = new Account(pk);
            _logger.LogInformation("Derived address: " + account.Address);

            var web3 = new Web3(_config.EthereumRpcUrl);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
            _logger.LogInformation("Sepolia ETH balance: " + Web3.Convert.FromWei(balance.Value));
        }
    }
}
