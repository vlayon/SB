using Data.Context;
using Data.Entities;

namespace Data.Repositories
{
    public class PairRepository : IPairRepository
    {
        private readonly SnippingBotDbContext _context;

        public PairRepository(SnippingBotDbContext context)
        {
            _context = context;
        }

        public async Task<Pair> CreateAsync(string token0, string token1, string pairAddress)
        {
            var pair = new Pair
            {
                FirstTokenAddress = token0,
                SecondTokenAddress = token1,
                PairAddress = pairAddress
            };

            _context.Pairs.Add(pair);
            await _context.SaveChangesAsync();
            return pair;
        }

        Task<bool> IPairRepository.ExistsAsync(string pairAddress)
        {
            throw new NotImplementedException();
        }

        Task<Pair?> IPairRepository.GetByAddressAsync(string pairAddress)
        {
            throw new NotImplementedException();
        }

        Task<List<Pair>> IPairRepository.GetRecentAsync(int count)
        {
            throw new NotImplementedException();
        }

        // Other methods...
    }
}
