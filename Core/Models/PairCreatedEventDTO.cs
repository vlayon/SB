using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Core.Models
{
    // DTO for PairCreated event from Uniswap V2 Factory
    [Event("PairCreated")]
    public class PairCreatedEventDTO
    {
        [Parameter("address", "token0", 1, true)]
        public string Token0 { get; set; }

        [Parameter("address", "token1", 2, true)]
        public string Token1 { get; set; }

        [Parameter("address", "pair", 3, false)]
        public string Pair { get; set; }

        [Parameter("uint256", "", 4, false)]
        public BigInteger PairIndex { get; set; }
    }
}
