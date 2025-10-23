using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Data.Context
{
    public class SnippingBotDbContext : DbContext
    {
        public SnippingBotDbContext(DbContextOptions<SnippingBotDbContext> options) : base(options)
        {
        }

        public DbSet<Pair> Pairs { get; set; }
        public DbSet<Trade> Trades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<Trade>()
                .HasOne(t => t.Pair)
                .WithMany(p => p.Trades)
                .HasForeignKey(t => t.PairId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes for performance
            modelBuilder.Entity<Pair>()
                .HasIndex(p => p.PairAddress)
                .IsUnique();

            modelBuilder.Entity<Pair>()
                .HasIndex(p => new { p.FirstTokenAddress, p.SecondTokenAddress });
        }
    }
}
