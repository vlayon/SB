
using Core;
using Core.Models;
using Core.Services;
using Microsoft.Extensions.Logging;

namespace Tests.Integration
{
    /// <summary>
    /// Integration tests for manual trading using NUnit
    /// These tests interact with actual blockchain (Sepolia testnet)
    /// Mark with [Ignore("Integration test")] to skip by default
    /// </summary>
    [TestFixture]
    public class ManualTradeTests
    {
        private BotConfiguration _config;
        private ILogger<TokenTrader> _traderLogger;
        private ILogger<PriceMonitor> _monitorLogger;
        private ILogger<HoneypotChecker> _honeypotLogger;

        [SetUp]
        public void Setup()
        {
            _config = LoadTestConfiguration();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            _traderLogger = loggerFactory.CreateLogger<TokenTrader>();
            _monitorLogger = loggerFactory.CreateLogger<PriceMonitor>();
            _honeypotLogger = loggerFactory.CreateLogger<HoneypotChecker>();
        }

        private BotConfiguration LoadTestConfiguration()
        {
            // Load from environment variables or use test defaults
            return new BotConfiguration
            {
                EthereumRpcUrl = Environment.GetEnvironmentVariable("TEST_RPC_URL")
                    ?? "https://sepolia.infura.io/v3/YOUR_KEY",
                //PrivateKey = Environment.GetEnvironmentVariable("TEST_PRIVATE_KEY")
                //    ?? throw new InvalidOperationException("TEST_PRIVATE_KEY not set"),
                //WalletAddress = Environment.GetEnvironmentVariable("TEST_WALLET_ADDRESS")
                //    ?? throw new InvalidOperationException("TEST_WALLET_ADDRESS not set"),
                UniswapV2FactoryAddress = "0x7E0987E5b3a30e3f2828572Bb659A548460a3003",
                UniswapV2RouterAddress = "0xC532a74256D3Db42D0Bf7a0400fEFDbad7694008",
                WethAddress = "0x7b79995e5f793A07Bc00c21412e50Ecae098E7f9",
                EthToUsePerTrade = 0.0001m, // Very small for testing
                SlippageTolerancePercent = 15m,
                TargetGainPercent = 50m,
                StopLossPercent = 30m,
                MaxHoldTimeSeconds = 60, // 1 minute for testing
                PriceCheckIntervalSeconds = 5,
                GasPriceMultiplier = 1.2m,
                MaxGasPrice = 500,
                GasLimit = 300000,
                EnableHoneypotCheck = true,
                MinLiquidityEth = 0.1m,
                OnlyTradeWithWeth = true
            };
        }

        //[Test]
        //[Ignore("Manual integration test - requires real testnet ETH")]
        //public async Task ManualTrade_WithKnownToken_ShouldExecuteBuyAndSell()
        //{
        //    // Arrange
        //    var tokenAddress = "0x1c7D4B196Cb0C7B01d743Fbc6116a902379C7238"; // Example Sepolia USDC
        //    var pairAddress = await FindPairAddressAsync(tokenAddress, _config.WethAddress);

        //    Assert.That(pairAddress, Is.Not.Null.And.Not.Empty, "Should find pair for test token");

        //    var tokenInfo = new TokenInfo
        //    {
        //        Address = tokenAddress,
        //        PairAddress = pairAddress,
        //        Token0 = tokenAddress,
        //        Token1 = _config.WethAddress,
        //        DetectedAt = DateTime.UtcNow,
        //        IsWethPair = true
        //    };

        //    var trader = new TokenTrader(_config, _traderLogger);
        //    var monitor = new PriceMonitor(_config, _monitorLogger, trader);
        //    var honeypotChecker = new HoneypotChecker(_config, _honeypotLogger);

        //    // Act - Safety check
        //    var isSafe = await honeypotChecker.CheckTokenSafetyAsync(tokenAddress, pairAddress);
        //    Assert.That(isSafe, Is.True, "Token should pass safety checks");

        //    // Act - Buy
        //    var buyResult = await trader.ExecuteBuyOrderAsync(tokenInfo);

        //    // Assert Buy
        //    Assert.That(buyResult.Success, Is.True, $"Buy should succeed: {buyResult.ErrorMessage}");
        //    Assert.That(buyResult.TransactionHash, Is.Not.Empty);
        //    Assert.That(buyResult.Amount, Is.GreaterThan(0), "Should receive tokens");

        //    // Act - Monitor and Sell
        //    await monitor.MonitorAndSellAsync(tokenInfo);

        //    // Note: Full assertion would require checking if sell executed
        //    // This is a basic smoke test to verify the flow works
        //}

        [Test]
        [Ignore("Manual integration test")]
        public async Task HoneypotCheck_WithValidToken_ShouldPass()
        {
            // Arrange
            var tokenAddress = "0x1c7D4B196Cb0C7B01d743Fbc6116a902379C7238";
            var pairAddress = await FindPairAddressAsync(tokenAddress, _config.WethAddress);
            var honeypotChecker = new HoneypotChecker(_config, _honeypotLogger);

            // Act
            var isSafe = await honeypotChecker.CheckTokenSafetyAsync(tokenAddress, pairAddress);

            // Assert
            Assert.That(isSafe, Is.True, "Known good token should pass safety checks");
        }

        [Test]
        public void SimulateNewPairDetection_WithMockData_ShouldIdentifyCorrectly()
        {
            // Arrange - Simulate a PairCreated event
            var mockEvent = new PairCreatedEventDTO
            {
                Token0 = "0x1234567890123456789012345678901234567890",
                Token1 = _config.WethAddress,
                Pair = "0xABCDEF1234567890ABCDEF1234567890ABCDEF12",
                PairIndex = new System.Numerics.BigInteger(100)
            };

            // Act - Check if it's a WETH pair
            bool isWethPair = mockEvent.Token0.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase) ||
                              mockEvent.Token1.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase);

            // Determine target token
            string targetToken = mockEvent.Token0.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase)
                ? mockEvent.Token1 : mockEvent.Token0;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(isWethPair, Is.True, "Should identify as WETH pair");
                Assert.That(targetToken, Is.EqualTo("0x1234567890123456789012345678901234567890"));
                Assert.That(mockEvent.Pair, Is.Not.Empty);
            });
        }

        [TestCase(0.001, 10, 0.0009)]  // 10% slippage
        [TestCase(0.001, 5, 0.00095)]  // 5% slippage
        [TestCase(0.001, 20, 0.0008)]  // 20% slippage
        public void CalculateMinAmountOut_WithSlippage_ShouldBeCorrect(
            decimal expectedAmount, decimal slippage, decimal expectedMin)
        {
            // Act
            var minAmount = expectedAmount * (1 - slippage / 100);

            // Assert
            Assert.That(minAmount, Is.EqualTo(expectedMin));
        }

        [Test]
        public void TokenInfo_WhenCreated_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var tokenInfo = new TokenInfo
            {
                Address = "0x1234567890123456789012345678901234567890",
                PairAddress = "0xABCDEF1234567890ABCDEF1234567890ABCDEF12",
                Token0 = "0x1234567890123456789012345678901234567890",
                Token1 = _config.WethAddress,
                DetectedAt = DateTime.UtcNow,
                IsWethPair = true,
                AmountBought = 100m,
                BuyPrice = 0.00001m
            };

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(tokenInfo.Address, Is.Not.Null);
                Assert.That(tokenInfo.IsWethPair, Is.True);
                Assert.That(tokenInfo.AmountBought, Is.EqualTo(100m));
                Assert.That(tokenInfo.BuyPrice, Is.EqualTo(0.00001m));
            });
        }

        private async Task<string> FindPairAddressAsync(string tokenA, string tokenB)
        {
            // Helper for integration tests - calls Uniswap factory getPair
            const string GET_PAIR_ABI = @"[{
                'constant': true,
                'inputs': [
                    {'name': 'tokenA', 'type': 'address'},
                    {'name': 'tokenB', 'type': 'address'}
                ],
                'name': 'getPair',
                'outputs': [{'name': 'pair', 'type': 'address'}],
                'type': 'function'
            }]";

            var web3 = new Nethereum.Web3.Web3(_config.EthereumRpcUrl);
            var factoryContract = web3.Eth.GetContract(GET_PAIR_ABI, _config.UniswapV2FactoryAddress);
            var getPairFunction = factoryContract.GetFunction("getPair");

            try
            {
                var pairAddress = await getPairFunction.CallAsync<string>(tokenA, tokenB);

                if (pairAddress == "0x0000000000000000000000000000000000000000")
                {
                    return null;
                }

                return pairAddress;
            }
            catch
            {
                return null;
            }
        }
    }
}
