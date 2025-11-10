using Nethereum.Hex.HexTypes;

namespace Core.Models
{
    public class TradeResult
    {
        public string TokenAddressIn { get; set; }

        public string TokenAddressOut { get; set; }

        public HexBigInteger Price { get; set; }

        public decimal Amount { get; set; }

        public string AmountDimension { get; set; }

        public bool Success { get; set; }

        public string TransactionHash { get; set; }

        public string ErrorMessage { get; set; }

        public decimal GasUsed { get; set; }
    }
}
