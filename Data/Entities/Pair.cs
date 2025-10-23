using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace Data.Entities
{
    public class Pair
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(42)]
        public string FirstTokenAddress { get; set; }

        [Required]
        [StringLength(42)]
        public string SecondTokenAddress { get; set; }

        [Required]
        [StringLength(42)]
        public string PairAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
    }
}
