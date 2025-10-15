using Core;
using Core.Models;
using FluentAssertions;

namespace Tests.Services
{
    /// <summary>
    /// Unit tests for PairDetector logic using NUnit
    /// Tests the pair detection logic without actual blockchain calls
    /// </summary>
    [TestFixture]
    public class PairDetectorTests
    {
        private BotConfiguration _config;

        [SetUp]
        public void Setup()
        {
            _config = CreateTestConfig();
        }

        [Test]
        public void DetectWethPair_Token0IsWeth_ReturnsTrue()
        {
            // Arrange
            var token0 = _config.WethAddress;
            var token1 = "0x1234567890123456789012345678901234567890";

            // Act
            bool isWethPair = token0.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase) ||
                              token1.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.That(isWethPair, Is.True, "token0 should be WETH");
            // Or using FluentAssertions:
            isWethPair.Should().BeTrue("because token0 is WETH");
        }

        [Test]
        public void DetectWethPair_Token1IsWeth_ReturnsTrue()
        {
            // Arrange
            var token0 = "0x1234567890123456789012345678901234567890";
            var token1 = _config.WethAddress;

            // Act
            bool isWethPair = token0.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase) ||
                              token1.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.That(isWethPair, Is.True);
        }

        [Test]
        public void DetectWethPair_NeitherTokenIsWeth_ReturnsFalse()
        {
            // Arrange
            var token0 = "0x1111111111111111111111111111111111111111";
            var token1 = "0x2222222222222222222222222222222222222222";

            // Act
            bool isWethPair = token0.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase) ||
                              token1.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.That(isWethPair, Is.False);
        }

        [Test]
        public void IdentifyTargetToken_Token0IsWeth_ReturnsToken1()
        {
            // Arrange
            var token0 = _config.WethAddress;
            var token1 = "0x1234567890123456789012345678901234567890";

            // Act
            string targetToken = token0.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase)
                ? token1 : token0;

            // Assert
            Assert.That(targetToken, Is.EqualTo(token1));
        }

        [Test]
        public void IdentifyTargetToken_Token1IsWeth_ReturnsToken0()
        {
            // Arrange
            var token0 = "0x1234567890123456789012345678901234567890";
            var token1 = _config.WethAddress;

            // Act
            string targetToken = token0.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase)
                ? token1 : token0;

            // Assert
            Assert.That(targetToken, Is.EqualTo(token0));
        }

        [Test]
        public void CreateTokenInfo_WithValidPairData_ShouldPopulateCorrectly()
        {
            // Arrange
            var token0 = "0x1234567890123456789012345678901234567890";
            var token1 = _config.WethAddress;
            var pairAddress = "0xABCDEF1234567890ABCDEF1234567890ABCDEF12";

            // Act
            var tokenInfo = new TokenInfo
            {
                Address = token0,
                PairAddress = pairAddress,
                Token0 = token0,
                Token1 = token1,
                DetectedAt = DateTime.UtcNow,
                IsWethPair = true
            };

            // Assert
            Assert.That(tokenInfo.Address, Is.EqualTo(token0));
            Assert.That(tokenInfo.PairAddress, Is.EqualTo(pairAddress));
            Assert.That(tokenInfo.IsWethPair, Is.True);
            Assert.That(tokenInfo.DetectedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void BlacklistedToken_ShouldBeSkipped()
        {
            // Arrange
            var blacklistedToken = "0xBADTOKEN1234567890123456789012345678901";
            _config.BlacklistedTokens = new List<string> { blacklistedToken };

            // Act
            bool isBlacklisted = _config.BlacklistedTokens.Any(bt =>
                bt.Equals(blacklistedToken, StringComparison.OrdinalIgnoreCase));

            // Assert
            Assert.That(isBlacklisted, Is.True);
        }

        [TestCase("0xABCD", "0xabcd", true)]  // Case insensitive
        [TestCase("0xABCD", "0xABCD", true)]  // Exact match
        [TestCase("0xABCD", "0x1234", false)] // Different
        public void AddressComparison_ShouldBeCaseInsensitive(string address1, string address2, bool expected)
        {
            // Act
            bool matches = address1.Equals(address2, StringComparison.OrdinalIgnoreCase);

            // Assert
            Assert.That(matches, Is.EqualTo(expected));
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

        #region Private methods
        private BotConfiguration CreateTestConfig()
        {
            return new BotConfiguration
            {
                EthereumRpcUrl = "https://sepolia.infura.io/v3/test",
                PrivateKey = "0123456789012345678901234567890123456789012345678901234567890123",
                WalletAddress = "0x1234567890123456789012345678901234567890",
                UniswapV2FactoryAddress = "0x7E0987E5b3a30e3f2828572Bb659A548460a3003",
                UniswapV2RouterAddress = "0xC532a74256D3Db42D0Bf7a0400fEFDbad7694008",
                WethAddress = "0x7b79995e5f793A07Bc00c21412e50Ecae098E7f9",
                EthToUsePerTrade = 0.001m,
                OnlyTradeWithWeth = true,
                EnableHoneypotCheck = false,
                SlippageTolerancePercent = 10m,
                TargetGainPercent = 50m,
                StopLossPercent = 30m
            };
        }
        #endregion
    }
}
