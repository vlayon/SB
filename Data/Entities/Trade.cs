using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Trade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PairId { get; set; }

        [Column(TypeName = "decimal(18,8)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(38,18)")]
        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Pair Pair { get; set; }

        // Additional properties from TradeResult

        [Required]
        [StringLength(42)]
        public string TokenAddressIn { get; set; }

        [Required]
        [StringLength(42)]
        public string TokenAddressOut { get; set; }

        // AmountDimension (e.g. "ETH", "TOKEN")
        [StringLength(50)]
        public string AmountDimension { get; set; }

        public bool Success { get; set; }

        [StringLength(66)]
        public string TransactionHash { get; set; }

        // Store error messages if the trade failed; nullable by design
        [StringLength(1000)]
        public string ErrorMessage { get; set; }

        // Gas used for the transaction
        [Column(TypeName = "decimal(18,8)")]
        public decimal GasUsed { get; set; }
    }
}
