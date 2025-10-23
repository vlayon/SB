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
    }
}
