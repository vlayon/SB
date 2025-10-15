namespace Core.Models
{
    public class TokenInfo
    {
        public string Address { get; set; }
        public string PairAddress { get; set; }
        public string Token0 { get; set; }
        public string Token1 { get; set; }
        public DateTime DetectedAt { get; set; }
        public decimal BuyPrice { get; set; }
        public decimal AmountBought { get; set; }
        public bool IsWethPair { get; set; }
    }
}
