using Microsoft.EntityFrameworkCore;
using Data.Context;
using Data.Entities;

namespace Data.Repositories
{
    public interface IPairRepository : IRepository<Pair>
    {
        Task<Pair> CreatePairAsync(string token0, string token1, string pairAddress);
        Task<bool> ExistsAsync(string pairAddress);

        Task<Pair> GetPairAsync(string pairAddress);
    }

    public class PairRepository : Repository<Pair>, IPairRepository
    {
        public PairRepository(SnippingBotDbContext context) : base(context) { }

        public async Task<Pair> CreatePairAsync(string token0, string token1, string pairAddress)
        {
            var pair = new Pair
            {
                FirstTokenAddress = token0,
                SecondTokenAddress = token1,
                PairAddress = pairAddress,
                CreatedAt = DateTime.UtcNow
            };

            _dbSet.Add(pair);
            await _context.SaveChangesAsync();
            return pair;
        }

        public Task<bool> ExistsAsync(string pairAddress) =>
            _dbSet.AnyAsync(p => p.PairAddress == pairAddress);

        public Task<Pair> GetPairAsync(string pairAddress)
        {
          return _dbSet.FirstOrDefaultAsync(p => p.PairAddress == pairAddress);
        }
    }
}