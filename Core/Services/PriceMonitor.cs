using Core.Models;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using System.Numerics;

namespace Core.Services
{
    public class PriceMonitor
    {
        private readonly Web3 _web3;
        private readonly BotConfiguration _config;
        private readonly ILogger<PriceMonitor> _logger;
        private readonly TokenTrader _trader;

        private const string ROUTER_ABI = @"[
            {
                'inputs': [
                    {'internalType': 'uint256', 'name': 'amountIn', 'type': 'uint256'},
                    {'internalType': 'address[]', 'name': 'path', 'type': 'address[]'}
                ],
                'name': 'getAmountsOut',
                'outputs': [{'internalType': 'uint256[]', 'name': 'amounts', 'type': 'uint256[]'}],
                'stateMutability': 'view',
                'type': 'function'
            }
        ]";

        public PriceMonitor(
            BotConfiguration config,
            ILogger<PriceMonitor> logger,
            TokenTrader trader)
        {
            _config = config;
            _logger = logger;
            _trader = trader;
            _web3 = new Web3(config.EthereumRpcUrl);
        }

        public async Task MonitorAndSellAsync(TokenInfo tokenInfo)
        {
            _logger.LogInformation("📊 Starting price monitoring for {Token}", tokenInfo.Address);

            var startTime = DateTime.UtcNow;
            var maxHoldTime = TimeSpan.FromSeconds(_config.MaxHoldTimeSeconds);
            var checkInterval = TimeSpan.FromSeconds(_config.PriceCheckIntervalSeconds);

            try
            {
                while (DateTime.UtcNow - startTime < maxHoldTime)
                {
                    // Get current price
                    var currentPriceData = await GetCurrentPriceAsync(tokenInfo);

                    if (currentPriceData.success)
                    {
                        var priceChangePercent = ((currentPriceData.ethValue - _config.EthToUsePerTrade) /
                                                  _config.EthToUsePerTrade) * 100;

                        _logger.LogInformation("   Current Value: {Value} ETH | Change: {Change:F2}%",
                            currentPriceData.ethValue, priceChangePercent);

                        // Check for target gain
                        if (priceChangePercent >= (decimal)_config.TargetGainPercent)
                        {
                            _logger.LogInformation("🎯 Target gain reached: {Percent:F2}%", priceChangePercent);
                            await _trader.ExecuteSellOrderAsync(tokenInfo, $"Target gain ({priceChangePercent:F2}%)");
                            return;
                        }

                        // Check for stop loss
                        if (priceChangePercent <= -(decimal)_config.StopLossPercent)
                        {
                            _logger.LogWarning("🛑 Stop loss triggered: {Percent:F2}%", priceChangePercent);
                            await _trader.ExecuteSellOrderAsync(tokenInfo, $"Stop loss ({priceChangePercent:F2}%)");
                            return;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("⚠️  Could not fetch price - will retry");
                    }

                    // Wait before next check
                    await Task.Delay(checkInterval);
                }

                // Max hold time reached
                _logger.LogInformation("⏰ Max hold time reached - selling at current price");
                await _trader.ExecuteSellOrderAsync(tokenInfo, "Max hold time reached");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring price - attempting emergency sell");
                try
                {
                    await _trader.ExecuteSellOrderAsync(tokenInfo, "Emergency sell due to error");
                }
                catch (Exception sellEx)
                {
                    _logger.LogError(sellEx, "Emergency sell also failed!");
                }
            }
        }

        private async Task<(bool success, decimal ethValue)> GetCurrentPriceAsync(TokenInfo tokenInfo)
        {
            try
            {
                var routerContract = _web3.Eth.GetContract(ROUTER_ABI, _config.UniswapV2RouterAddress);
                var getAmountsOutFunction = routerContract.GetFunction("getAmountsOut");

                // Get the amount of tokens we have
                var amountIn = Web3.Convert.ToWei(tokenInfo.AmountBought);

                // Build path: Token -> WETH
                var path = new List<string> { tokenInfo.Address, _config.WethAddress };

                // Get expected output
                var amounts = await getAmountsOutFunction.CallAsync<List<BigInteger>>(amountIn, path);
                var ethValue = Web3.Convert.FromWei(amounts[1]);

                return (true, ethValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching current price");
                return (false, 0);
            }
        }
    }
}
