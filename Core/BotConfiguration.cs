namespace Core
{
    public class BotConfiguration
    {
        // Connection settings
        public string EthereumRpcUrl { get; set; }
        public string PrivateKey { get; set; }
        public string WalletAddress { get; set; }

        // Contract addresses
        public string UniswapV2FactoryAddress { get; set; } = "0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f";
        public string UniswapV2RouterAddress { get; set; } = "0x7a250d5630B4cF539739dF2C5dAcb4c659F2488D";
        public string WethAddress { get; set; } = "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2";

        // Trading parameters
        public decimal EthToUsePerTrade { get; set; } = 0.01m;
        public decimal SlippageTolerancePercent { get; set; } = 10m;
        public decimal TargetGainPercent { get; set; } = 50m;
        public decimal StopLossPercent { get; set; } = 30m;
        public int MaxHoldTimeSeconds { get; set; } = 300; // 5 minutes
        public int PriceCheckIntervalSeconds { get; set; } = 5;

        // Gas settings
        public decimal GasPriceMultiplier { get; set; } = 1.2m; // 20% above current
        public int MaxGasPrice { get; set; } = 500; // Gwei
        public int GasLimit { get; set; } = 300000;

        // Safety settings
        public bool EnableHoneypotCheck { get; set; } = true;
        public decimal MinLiquidityEth { get; set; } = 1m; // Minimum 1 ETH liquidity
        public int MinHoldersBeforeTrade { get; set; } = 0;
        public bool OnlyTradeWithWeth { get; set; } = true; // Only trade pairs with WETH

        // Advanced settings
        public bool EnableMempoolMonitoring { get; set; } = false;
        public int MaxConcurrentTrades { get; set; } = 3;
        public List<string> BlacklistedTokens { get; set; } = new List<string>();

        // Manual test mode
        public bool ManualTestMode { get; set; } = false;
        public string ManualTestTokenAddress { get; set; }
        public string ManualTestPairAddress { get; set; }

        // Mock test mode - simulates pair detection without real trading
        public bool MockPairDetectionMode { get; set; } = false;
        public string MockTokenAddress { get; set; }
        public string MockPairAddress { get; set; }
        public bool MockExecuteRealTrade { get; set; } = false;
    }
}
