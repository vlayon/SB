using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Data.Context
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SnippingBotDbContext>
    {
        public SnippingBotDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SnippingBotDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SnippBotDb;Trusted_Connection=true;");

            return new SnippingBotDbContext(optionsBuilder.Options);
        }
    }
}
