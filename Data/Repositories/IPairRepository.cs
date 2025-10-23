using Data.Context;
using Data.Entities;

namespace Data.Repositories
{
    public interface IPairRepository
    {
        Task<Pair> CreateAsync(string token0, string token1, string pairAddress);
        Task<Pair?> GetByAddressAsync(string pairAddress);
        Task<List<Pair>> GetRecentAsync(int count = 10);
        Task<bool> ExistsAsync(string pairAddress);
    }       
}
