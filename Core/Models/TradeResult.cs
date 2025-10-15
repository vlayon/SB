namespace Core.Models
{
    public class TradeResult
    {
        public bool Success { get; set; }
        public string TransactionHash { get; set; }
        public string ErrorMessage { get; set; }
        public decimal AmountOut { get; set; }
        public decimal GasUsed { get; set; }
    }
}
