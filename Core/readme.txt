Crypto Snipping Bot (C#)
A C# bot that automatically detects new token pairs on Uniswap V2, purchases them, and sells at target profit or stop-loss.
⚠️ IMPORTANT WARNINGS

This is HIGH RISK - You can lose all your ETH very quickly
Most new tokens are SCAMS (rugpulls, honeypots)
Test on testnet first (Goerli or Sepolia)
Start with TINY amounts (0.001 ETH or less)
Never share your private key - Keep it secure!

📋 Prerequisites

.NET 8.0 SDK or later
An Ethereum RPC provider (Alchemy, Infura, or your own node)
A wallet with ETH for trading + gas fees
Basic understanding of smart contracts and DeFi

🚀 Setup Instructions
1. Install .NET
Download and install from: https://dotnet.microsoft.com/download
2. Create the Project
bashmkdir SnipingBot
cd SnipingBot
dotnet new console
3. Add Files
Copy all the provided files into your project:

Program.cs
Models/BotConfiguration.cs
Models/TokenInfo.cs (included in BotConfiguration.cs)
Services/PairDetector.cs
Services/TokenTrader.cs
Services/PriceMonitor.cs
Services/HoneypotChecker.cs
appsettings.json
SnipingBot.csproj

4. Configure appsettings.json
Edit appsettings.json and fill in:
json{
  "BotConfig": {
    "EthereumRpcUrl": "https://eth-mainnet.g.alchemy.com/v2/YOUR_API_KEY",
    "PrivateKey": "YOUR_PRIVATE_KEY_WITHOUT_0x",
    "WalletAddress": "0xYourWalletAddress",
    ...
  }
}
Getting an RPC URL:

Go to Alchemy or Infura
Create a free account
Create a new app for Ethereum Mainnet
Copy the HTTPS URL

Your Private Key:

Export from MetaMask: Settings → Security & Privacy → Reveal Secret Recovery Phrase
⚠️ NEVER share this or commit it to Git!

5. Install Dependencies
bashdotnet restore
6. Build the Project
bashdotnet build
7. Run the Bot
bashdotnet run
📊 Configuration Options
Trading Parameters
ParameterDefaultDescriptionEthToUsePerTrade0.01Amount of ETH to spend per tradeSlippageTolerancePercent10Maximum slippage allowed (%)TargetGainPercent50Sell when profit reaches this %StopLossPercent30Sell when loss reaches this %MaxHoldTimeSeconds300Max time to hold token (seconds)PriceCheckIntervalSeconds5How often to check price
Safety Settings
ParameterDefaultDescriptionEnableHoneypotChecktrueRun safety checks before buyingMinLiquidityEth1.0Minimum liquidity required (ETH)OnlyTradeWithWethtrueOnly trade WETH pairsMaxGasPrice500Maximum gas price (Gwei)
🎯 How It Works

Detection: Listens for PairCreated events on Uniswap V2 Factory
Validation: Checks if pair contains WETH and has sufficient liquidity
Safety Check: Validates token isn't a honeypot or scam
Buy: Executes swap using Uniswap V2 Router
Monitor: Continuously checks price every N seconds
Sell: Automatically sells when:

Target gain reached (50% profit by default)
Stop loss hit (30% loss by default)
Max hold time exceeded (5 minutes by default)



📝 Example Output
=== Crypto Snipping Bot Starting ===

[INFO] Starting to listen for new pairs on Uniswap V2 Factory: 0x5C69...
[INFO] Successfully created event filter. Listening for new pairs...

[INFO] 🆕 New Pair Detected!
[INFO]    Pair: 0x1234...
[INFO]    Token0: 0xC02a... (WETH)
[INFO]    Token1: 0x5678...
[INFO] 🔍 Running honeypot check...
[INFO]    ✅ Liquidity: 5.2 ETH
[INFO]    ✅ Total Supply: 1,000,000
[INFO]    ✅ Token passed safety checks
[INFO] ✅ All checks passed - executing trade...

[INFO] 💰 Executing BUY order for 0x5678...
[INFO]    Amount In: 0.01 ETH
[INFO]    Expected Out: ~12345 tokens
[INFO] ✅ BUY SUCCESS!
[INFO]    TX: 0xabcd...
[INFO]    Gas Used: 185234

[INFO] 📊 Starting price monitoring for 0x5678...
[INFO]    Current Value: 0.012 ETH | Change: 20.00%
[INFO]    Current Value: 0.015 ETH | Change: 50.00%
[INFO] 🎯 Target gain reached: 50.00%

[INFO] 💸 Executing SELL order for 0x5678...
[INFO]    Selling 12345 tokens
[INFO]    Expected ETH: 0.015
[INFO]    P/L: 0.005 ETH (50.00%)
[INFO] ✅ SELL SUCCESS!
[INFO]    TX: 0xef01...
⚙️ Advanced Configuration
Testing on Testnet
Change the addresses in appsettings.json:
json{
  "EthereumRpcUrl": "https://eth-goerli.g.alchemy.com/v2/YOUR_KEY",
  "UniswapV2FactoryAddress": "0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f",
  "UniswapV2RouterAddress": "0x7a250d5630B4cF539739dF2C5dAcb4c659F2488D",
  "WethAddress": "0xB4FBF271143F4FBf7B91A5ded31805e42b2208d6"
}
Get testnet ETH from: https://goerlifaucet.com/
Blacklist Tokens
Add scam tokens to avoid:
json{
  "BlacklistedTokens": [
    "0xScamToken1Address",
    "0xScamToken2Address"
  ]
}
🛡️ Security Best Practices

Use a dedicated wallet - Don't use your main wallet
Keep minimal ETH - Only what you're willing to lose
Never commit private keys - Add appsettings.json to .gitignore
Use environment variables - For production, use env vars instead of config file
Monitor logs - Check for failed transactions and errors
Test thoroughly - Always test on testnet first

🐛 Troubleshooting
"Transaction reverted"

Token might be a honeypot (can't sell)
Slippage too low - increase SlippageTolerancePercent
Gas price too low - increase GasPriceMultiplier

"Insufficient funds"

Check wallet has enough ETH
Reduce EthToUsePerTrade

"No new pairs detected"

Check RPC URL is correct
Ensure you're connected to correct network
New pairs are rare - might need to wait hours

"Gas price too high"

Increase MaxGasPrice in config
Or wait for gas prices to drop

📚 Further Improvements
This is a basic implementation. For production use, consider:

MEV Protection - Use Flashbots to prevent front-running
Better Honeypot Detection - Integrate with honeypot.is API
Token Analysis - Check contract code, ownership, etc.
Multiple DEXs - Monitor PancakeSwap, SushiSwap, etc.
Database - Store trade history
Telegram Notifications - Get alerts on trades
Web Dashboard - Monitor bot status in real-time

⚖️ Legal Disclaimer
This software is for EDUCATIONAL PURPOSES ONLY. Trading cryptocurrencies carries significant risk. You can lose all your money. The authors are not responsible for any financial losses. Use at your own risk.
📄 License
MIT License - Use freely but at your own risk!