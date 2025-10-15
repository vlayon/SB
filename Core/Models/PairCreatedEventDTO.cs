using System.Numerics;

namespace Core.Models
{
    // DTO for PairCreated event from Uniswap V2 Factory
    [Nethereum.ABI.FunctionEncoding.Attributes.Event("PairCreated")]
    public class PairCreatedEventDTO
    {
        [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("address", "token0", 1, true)]
        public string Token0 { get; set; }

        [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("address", "token1", 2, true)]
        public string Token1 { get; set; }

        [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("address", "pair", 3, false)]
        public string Pair { get; set; }

        [Nethereum.ABI.FunctionEncoding.Attributes.Parameter("uint256", "", 4, false)]
        public BigInteger PairIndex { get; set; }
    }
}
