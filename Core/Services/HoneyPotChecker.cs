using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using System.Numerics;

namespace Core.Services
{
    public class HoneypotChecker
    {
        private readonly Web3 _web3;
        private readonly BotConfiguration _config;
        private readonly ILogger<HoneypotChecker> _logger;

        private const string PAIR_ABI = @"[
            {
                'constant': true,
                'inputs': [],
                'name': 'getReserves',
                'outputs': [
                    {'name': 'reserve0', 'type': 'uint112'},
                    {'name': 'reserve1', 'type': 'uint112'},
                    {'name': 'blockTimestampLast', 'type': 'uint32'}
                ],
                'type': 'function'
            },
            {
                'constant': true,
                'inputs': [],
                'name': 'token0',
                'outputs': [{'name': '', 'type': 'address'}],
                'type': 'function'
            },
            {
                'constant': true,
                'inputs': [],
                'name': 'token1',
                'outputs': [{'name': '', 'type': 'address'}],
                'type': 'function'
            }
        ]";

        private const string ERC20_ABI = @"[
            {
                'constant': true,
                'inputs': [],
                'name': 'totalSupply',
                'outputs': [{'name': '', 'type': 'uint256'}],
                'type': 'function'
            },
            {
                'constant': true,
                'inputs': [],
                'name': 'decimals',
                'outputs': [{'name': '', 'type': 'uint8'}],
                'type': 'function'
            }
        ]";

        public HoneypotChecker(
            BotConfiguration config,
            ILogger<HoneypotChecker> logger)
        {
            _config = config;
            _logger = logger;
            _web3 = new Web3(config.EthereumRpcUrl);
        }

        public async Task<bool> CheckTokenSafetyAsync(string tokenAddress, string pairAddress)
        {
            try
            {
                // Check 1: Verify liquidity is sufficient
                var liquidityCheck = await CheckLiquidityAsync(pairAddress);
                if (!liquidityCheck.passed)
                {
                    _logger.LogWarning("   ❌ Liquidity check failed: {Reason}", liquidityCheck.reason);
                    return false;
                }
                _logger.LogInformation("   ✅ Liquidity: {Liquidity} ETH", liquidityCheck.ethLiquidity);

                // Check 2: Verify token has reasonable total supply
                var supplyCheck = await CheckTotalSupplyAsync(tokenAddress);
                if (!supplyCheck.passed)
                {
                    _logger.LogWarning("   ❌ Supply check failed: {Reason}", supplyCheck.reason);
                    return false;
                }
                _logger.LogInformation("   ✅ Total Supply: {Supply}", supplyCheck.totalSupply);

                // Check 3: Try to simulate a small buy/sell to detect honeypots
                // Note: This is simplified - a real implementation would use a simulator contract
                _logger.LogInformation("   ⚠️  Honeypot simulation not implemented - proceeding with caution");

                _logger.LogInformation("   ✅ Token passed safety checks");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during safety check");
                return false;
            }
        }

        private async Task<(bool passed, string reason, decimal ethLiquidity)> CheckLiquidityAsync(string pairAddress)
        {
            try
            {
                var pairContract = _web3.Eth.GetContract(PAIR_ABI, pairAddress);

                // Get token addresses
                var token0Function = pairContract.GetFunction("token0");
                var token1Function = pairContract.GetFunction("token1");
                var token0 = await token0Function.CallAsync<string>();
                var token1 = await token1Function.CallAsync<string>();

                // Get reserves
                var getReservesFunction = pairContract.GetFunction("getReserves");
                var reserves = await getReservesFunction.CallDeserializingToObjectAsync<ReservesOutput>();

                // Determine which reserve is WETH
                bool isToken0Weth = token0.Equals(_config.WethAddress, StringComparison.OrdinalIgnoreCase);
                var wethReserve = isToken0Weth ? reserves.Reserve0 : reserves.Reserve1;
                var ethLiquidity = Web3.Convert.FromWei(wethReserve);

                if (ethLiquidity < _config.MinLiquidityEth)
                {
                    return (false, $"Liquidity too low: {ethLiquidity} ETH (min: {_config.MinLiquidityEth} ETH)", ethLiquidity);
                }

                return (true, "OK", ethLiquidity);
            }
            catch (Exception ex)
            {
                return (false, $"Error checking liquidity: {ex.Message}", 0);
            }
        }

        private async Task<(bool passed, string reason, string totalSupply)> CheckTotalSupplyAsync(string tokenAddress)
        {
            try
            {
                var tokenContract = _web3.Eth.GetContract(ERC20_ABI, tokenAddress);

                // Get decimals
                var decimalsFunction = tokenContract.GetFunction("decimals");
                var decimals = await decimalsFunction.CallAsync<byte>();

                // Get total supply
                var totalSupplyFunction = tokenContract.GetFunction("totalSupply");
                var totalSupply = await totalSupplyFunction.CallAsync<BigInteger>();

                // Check if supply is reasonable (not 0, not extremely high)
                if (totalSupply == 0)
                {
                    return (false, "Total supply is 0", "0");
                }

                // Convert to readable format
                var divisor = BigInteger.Pow(10, decimals);
                var readableSupply = (decimal)totalSupply / (decimal)divisor;

                // Check if supply is suspiciously high (> 1 quadrillion tokens)
                if (readableSupply > 1_000_000_000_000_000m)
                {
                    return (false, "Supply suspiciously high", readableSupply.ToString("N0"));
                }

                return (true, "OK", readableSupply.ToString("N0"));
            }
            catch (Exception ex)
            {
                return (false, $"Error checking supply: {ex.Message}", "Unknown");
            }
        }

        [Nethereum.ABI.FunctionEncoding.Attributes.FunctionOutput]
        private class ReservesOutput
        {
            [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("uint112", "reserve0", 1)]
            public BigInteger Reserve0 { get; set; }

            [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("uint112", "reserve1", 2)]
            public BigInteger Reserve1 { get; set; }

            [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("uint32", "blockTimestampLast", 3)]
            public uint BlockTimestampLast { get; set; }
        }
    }
}
