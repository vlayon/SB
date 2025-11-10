using Data.Context;
using Data.Entities;

namespace Data.Repositories
{
    public interface ITradeRepository : IRepository<Trade>
    {
        Task<Trade> CreateTradeAsync(int pairId,
            decimal price,
            decimal amount,
            string amountDimension,
            string tokenAddressIn,
            string tokenAddressOut,
            bool success,
            string transactionHash,
            decimal gasUsed,
            string errorMessage);

    }

    public class TradeRepository : Repository<Trade>, ITradeRepository
    {
        public TradeRepository(SnippingBotDbContext context) : base(context)
        {
        }
        public async Task<Trade> CreateTradeAsync(int pairId, 
            decimal price, 
            decimal amount,
            string amountDimension,
            string tokenAddressIn,
            string tokenAddressOut,
            bool success,
            string transactionHash,
            decimal gasUsed,
            string errorMessage =null            
            )
        {
            var trade = new Trade
            {
                PairId = pairId,
                Price = price,
                Amount = amount,
                AmountDimension = amountDimension,
                TokenAddressIn = tokenAddressIn,
                TokenAddressOut = tokenAddressOut,
                Success = success,
                TransactionHash = transactionHash,
                ErrorMessage = errorMessage,
                GasUsed = gasUsed,
                CreatedAt = DateTime.UtcNow
            };
            await _dbSet.AddAsync(trade);
            await _context.SaveChangesAsync();
            return trade;
        }
    
    }
}
