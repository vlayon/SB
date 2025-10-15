using Core.Models;
using Microsoft.Extensions.Logging;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;

namespace Core.Services
{
    public class TokenTrader
    {
        private readonly Web3 _web3;
        private readonly BotConfiguration _config;
        private readonly ILogger<TokenTrader> _logger;
        //private readonly PriceMonitor _priceMonitor;
        private readonly Account _account;

        // Uniswap V2 Router ABI (simplified - key functions only)
        private const string ROUTER_ABI = @"[
            {
                'inputs': [
                    {'internalType': 'uint256', 'name': 'amountIn', 'type': 'uint256'},
                    {'internalType': 'uint256', 'name': 'amountOutMin', 'type': 'uint256'},
                    {'internalType': 'address[]', 'name': 'path', 'type': 'address[]'},
                    {'internalType': 'address', 'name': 'to', 'type': 'address'},
                    {'internalType': 'uint256', 'name': 'deadline', 'type': 'uint256'}
                ],
                'name': 'swapExactETHForTokens',
                'outputs': [{'internalType': 'uint256[]', 'name': 'amounts', 'type': 'uint256[]'}],
                'stateMutability': 'payable',
                'type': 'function'
            },
            {
                'inputs': [
                    {'internalType': 'uint256', 'name': 'amountIn', 'type': 'uint256'},
                    {'internalType': 'uint256', 'name': 'amountOutMin', 'type': 'uint256'},
                    {'internalType': 'address[]', 'name': 'path', 'type': 'address[]'},
                    {'internalType': 'address', 'name': 'to', 'type': 'address'},
                    {'internalType': 'uint256', 'name': 'deadline', 'type': 'uint256'}
                ],
                'name': 'swapExactTokensForETH',
                'outputs': [{'internalType': 'uint256[]', 'name': 'amounts', 'type': 'uint256[]'}],
                'stateMutability': 'nonpayable',
                'type': 'function'
            },
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

        private const string ERC20_ABI = @"[
            {
                'constant': false,
                'inputs': [
                    {'name': 'spender', 'type': 'address'},
                    {'name': 'amount', 'type': 'uint256'}
                ],
                'name': 'approve',
                'outputs': [{'name': '', 'type': 'bool'}],
                'type': 'function'
            },
            {
                'constant': true,
                'inputs': [{'name': 'account', 'type': 'address'}],
                'name': 'balanceOf',
                'outputs': [{'name': '', 'type': 'uint256'}],
                'type': 'function'
            }
        ]";

        public TokenTrader(
            BotConfiguration config,
            ILogger<TokenTrader> logger)
        {
            _config = config;
            _logger = logger;
            //_priceMonitor = priceMonitor;
            _account = new Account(config.PrivateKey);
            _web3 = new Web3(_account, config.EthereumRpcUrl);
        }

        public async Task<TradeResult> ExecuteBuyOrderAsync(TokenInfo tokenInfo)
        {
            try
            {
                _logger.LogInformation("💰 Executing BUY order for {Token}", tokenInfo.Address);

                var routerContract = _web3.Eth.GetContract(ROUTER_ABI, _config.UniswapV2RouterAddress);

                // Calculate amounts
                var amountInWei = Web3.Convert.ToWei(_config.EthToUsePerTrade);
                var deadline = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();

                // Build path: WETH -> Token
                var path = new List<string> { _config.WethAddress, tokenInfo.Address };

                // Get expected output with slippage
                var getAmountsOutFunction = routerContract.GetFunction("getAmountsOut");
                var amounts = await getAmountsOutFunction.CallAsync<List<BigInteger>>(amountInWei, path);
                var amountOutMin = amounts[1] * (100 - (int)_config.SlippageTolerancePercent) / 100;

                _logger.LogInformation("   Amount In: {AmountIn} ETH", _config.EthToUsePerTrade);
                _logger.LogInformation("   Expected Out: ~{AmountOut} tokens", Web3.Convert.FromWei(amounts[1]));
                _logger.LogInformation("   Min Out (slippage): {MinOut} tokens", Web3.Convert.FromWei(amountOutMin));

                // Get current gas price
                var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();
                var adjustedGasPrice = new BigInteger((decimal)gasPrice.Value * (decimal)_config.GasPriceMultiplier);
                var maxGasPrice = Web3.Convert.ToWei(_config.MaxGasPrice, Nethereum.Util.UnitConversion.EthUnit.Gwei);

                if (adjustedGasPrice > maxGasPrice)
                {
                    _logger.LogWarning("⚠️  Gas price too high: {GasPrice} Gwei (max: {MaxGas} Gwei)",
                        Web3.Convert.FromWei(adjustedGasPrice, Nethereum.Util.UnitConversion.EthUnit.Gwei),
                        _config.MaxGasPrice);
                    return new TradeResult { Success = false, ErrorMessage = "Gas price too high" };
                }

                // Execute swap
                var swapFunction = routerContract.GetFunction("swapExactETHForTokens");

                var txReceipt = await swapFunction.SendTransactionAndWaitForReceiptAsync(
                    from: _account.Address,
                    gas: new HexBigInteger(new BigInteger(_config.GasLimit)),
                    gasPrice: new HexBigInteger(adjustedGasPrice),
                    value: new HexBigInteger(amountInWei),
                    functionInput: new object[] { amountOutMin, path, _account.Address, deadline }
                );

                if (txReceipt.Status.Value == 1)
                {
                    _logger.LogInformation("✅ BUY SUCCESS!");
                    _logger.LogInformation("   TX: {TxHash}", txReceipt.TransactionHash);
                    _logger.LogInformation("   Gas Used: {Gas}", txReceipt.GasUsed.Value);

                    // Get actual token balance
                    var tokenBalance = await GetTokenBalanceAsync(tokenInfo.Address);
                    tokenInfo.AmountBought = tokenBalance;
                    tokenInfo.BuyPrice = _config.EthToUsePerTrade / tokenBalance; // ETH per token

                    _logger.LogInformation("✅ Purchase complete. Token balance: {Balance}", tokenBalance);

                    return new TradeResult
                    {
                        Success = true,
                        TransactionHash = txReceipt.TransactionHash,
                        AmountOut = tokenBalance,
                        GasUsed = (decimal)txReceipt.GasUsed.Value
                    };
                }
                else
                {
                    _logger.LogError("❌ BUY FAILED - Transaction reverted");
                    return new TradeResult { Success = false, ErrorMessage = "Transaction reverted" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error executing buy order");
                return new TradeResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<TradeResult> ExecuteSellOrderAsync(TokenInfo tokenInfo, string reason)
        {
            try
            {
                _logger.LogInformation("💸 Executing SELL order for {Token} - Reason: {Reason}",
                    tokenInfo.Address, reason);

                var routerContract = _web3.Eth.GetContract(ROUTER_ABI, _config.UniswapV2RouterAddress);

                // Get current token balance
                var tokenBalance = await GetTokenBalanceAsync(tokenInfo.Address);
                var amountIn = Web3.Convert.ToWei(tokenBalance);

                _logger.LogInformation("   Selling {Amount} tokens", tokenBalance);

                // Approve router to spend tokens
                await ApproveTokenAsync(tokenInfo.Address, _config.UniswapV2RouterAddress, amountIn);

                // Build path: Token -> WETH
                var path = new List<string> { tokenInfo.Address, _config.WethAddress };
                var deadline = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();

                // Get expected output
                var getAmountsOutFunction = routerContract.GetFunction("getAmountsOut");
                var amounts = await getAmountsOutFunction.CallAsync<List<BigInteger>>(amountIn, path);
                var amountOutMin = amounts[1] * (100 - (int)_config.SlippageTolerancePercent) / 100;

                var expectedEth = Web3.Convert.FromWei(amounts[1]);
                _logger.LogInformation("   Expected ETH: {ExpectedEth}", expectedEth);

                // Calculate profit/loss
                var profitLoss = expectedEth - _config.EthToUsePerTrade;
                var profitLossPercent = (profitLoss / _config.EthToUsePerTrade) * 100;
                _logger.LogInformation("   P/L: {ProfitLoss} ETH ({Percent:F2}%)",
                    profitLoss, profitLossPercent);

                // Execute swap
                var swapFunction = routerContract.GetFunction("swapExactTokensForETH");
                var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();
                var adjustedGasPrice = new BigInteger((decimal)gasPrice.Value * (decimal)_config.GasPriceMultiplier);

                var txReceipt = await swapFunction.SendTransactionAndWaitForReceiptAsync(
                    from: _account.Address,
                    gas: new HexBigInteger(new BigInteger(_config.GasLimit)),
                    gasPrice: new HexBigInteger(adjustedGasPrice),
                    value: null,
                    functionInput: new object[] { amountIn, amountOutMin, path, _account.Address, deadline }
                );

                if (txReceipt.Status.Value == 1)
                {
                    _logger.LogInformation("✅ SELL SUCCESS!");
                    _logger.LogInformation("   TX: {TxHash}", txReceipt.TransactionHash);

                    return new TradeResult
                    {
                        Success = true,
                        TransactionHash = txReceipt.TransactionHash,
                        AmountOut = expectedEth,
                        GasUsed = (decimal)txReceipt.GasUsed.Value
                    };
                }
                else
                {
                    _logger.LogError("❌ SELL FAILED - Transaction reverted");
                    return new TradeResult { Success = false, ErrorMessage = "Transaction reverted" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error executing sell order");
                return new TradeResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        private async Task<decimal> GetTokenBalanceAsync(string tokenAddress)
        {
            var tokenContract = _web3.Eth.GetContract(ERC20_ABI, tokenAddress);
            var balanceFunction = tokenContract.GetFunction("balanceOf");
            var balance = await balanceFunction.CallAsync<BigInteger>(_account.Address);
            return Web3.Convert.FromWei(balance);
        }

        private async Task ApproveTokenAsync(string tokenAddress, string spender, BigInteger amount)
        {
            var tokenContract = _web3.Eth.GetContract(ERC20_ABI, tokenAddress);
            var approveFunction = tokenContract.GetFunction("approve");

            // Estimate gas needed for approval
            var gas = await approveFunction.EstimateGasAsync(
                from: _account.Address,
                gas: null,
                value: null,
                functionInput: new object[] { spender, amount }
            );

            // Execute approval transaction
            var txReceipt = await approveFunction.SendTransactionAndWaitForReceiptAsync(
                from: _account.Address,
                gas: gas,
                gasPrice: null,
                value: null,
                functionInput: new object[] { spender, amount }
            );

            _logger.LogInformation("Token approved for trading");
        }
    }
}
