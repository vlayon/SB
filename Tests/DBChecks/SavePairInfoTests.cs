using Core.Models;
using Data.Context;
using Data.Repositories;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Numerics;

namespace Tests.DBChecks
{
    [TestFixture]
    public class SavePairInfoTests
    {
        private SnippingBotDbContext _context;
        private IPairRepository _pairRepository;

        [OneTimeSetUp]
        public void SetUp()
        {
            // Get the real DB context using the factory
            var factory = new DesignTimeDbContextFactory();
            _context = factory.CreateDbContext(Array.Empty<string>());
            _pairRepository = new PairRepository(_context);
        }

        [Test]
        public async Task When_CreatePair_Then_ItShouldBeRecordedInDB()
        {
            // Arrange
            var token0Address = "0x6B175474E89094C44Da98b954EedeAC495271d0F"; // Example DAI address
            var token1Address = "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2"; // Example WETH address
            var pairAddress = "0xA478c2975Ab1Ea89e8196811F51A7B7Ade33eB11";   // Example DAI-WETH pair



            //try
            //{
                // Act
                var dbPair = await _pairRepository.CreateAsync(token0Address, token1Address, pairAddress);

                // TODO: Assert
                //var savedPair = await _context.PairInfos
                //    .FirstOrDefaultAsync(p => p.PairAddress == pairAddress);

                //Assert.That(savedPair, Is.Not.Null);
                //Assert.That(savedPair.Token0, Is.EqualTo(token0Address));
                //Assert.That(savedPair.Token1, Is.EqualTo(token1Address));
            //}
            //finally
            //{
            //    // Optionally clean up the test data if needed
            //    var pair = await _context.PairInfos.FirstOrDefaultAsync(p => p.PairAddress == pairAddress);
            //    if (pair != null)
            //    {
            //        _context.PairInfos.Remove(pair);
            //        await _context.SaveChangesAsync();
            //    }
            //}
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }
    }
}
