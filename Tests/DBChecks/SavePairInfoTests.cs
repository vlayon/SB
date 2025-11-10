using Core.Models;
using Data.Context;
using Data.Entities;
using Data.Repositories;
using Microsoft.EntityFrameworkCore;
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
        private ITradeRepository _tradeRepository;
        string token0Address = "0x6B175474E89094C44Da98b954EedeAC495271d0F"; // Example DAI address
        string token1Address = "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2"; // Example WETH address
        string pairAddress = "0xA478c2975Ab1Ea89e8196811F51A7B7Ade33eB11"; // Example DAI-WETH pair


        [OneTimeSetUp]
        public async Task SetUp()
        {
            // Get the real DB context using the factory
            var factory = new DesignTimeDbContextFactory();
            _context = factory.CreateDbContext(Array.Empty<string>());
            _pairRepository = new PairRepository(_context);
            _tradeRepository = new TradeRepository(_context);
        }

        [SetUp]
        public async Task SetupEach()
        {
            await DeletePairIfExists(pairAddress);
        }

        [Test]
        public async Task When_CreatePair_Then_ItShouldBeRecordedInDB()
        {
            // Arrange

            // Act
            var dbPair = await _pairRepository.CreatePairAsync(token0Address, token1Address, pairAddress);

            var actualData = GetPairData(pairAddress);

            // Assert
            Assert.That(actualData.FirstTokenAddress, Is.EqualTo(token0Address));
            Assert.That(actualData.SecondTokenAddress, Is.EqualTo(token1Address));
            Assert.That(actualData.PairAddress, Is.EqualTo(pairAddress));
            Assert.That(actualData.CreatedAt.Date, Is.EqualTo(DateTime.Now.Date));

        }

        [Test]
        public async Task When_CreateATradesWithBuyOrder_Then_ItShouldBeRecordedInDB()
        {
            // Arrange

            // Act
            var trade = await _tradeRepository.CreateTrade();
            //var actualData = GetPairData(pairAddress);

            // Assert
            //Assert.That(actualData.FirstTokenAddress, Is.EqualTo(token0Address));
            //Assert.That(actualData.SecondTokenAddress, Is.EqualTo(token1Address));
            //Assert.That(actualData.PairAddress, Is.EqualTo(pairAddress));
            //Assert.That(actualData.CreatedAt.Date, Is.EqualTo(DateTime.Now.Date));

        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        #region Private Methods
        private async Task DeletePairIfExists(string pairAddress)
        {
            try
            {
                var existingPair = await _context.Pairs
                    .FirstOrDefaultAsync(p => p.PairAddress.Equals(pairAddress));
                if (existingPair != null)
                {
                    _context.Pairs.Remove(existingPair);
                    await _context.SaveChangesAsync();
                }
            } catch(Exception e)
            {
                Debug.WriteLine($"Error in DeletePairIfExists: {e.Message}");
                throw; // Re-throw to make the test fail with the actual error
            }
        }

        private Pair GetPairData(string pairAddress) => _context.Pairs
                .AsNoTracking()
                .FirstOrDefault(p => p.PairAddress == pairAddress);
        #endregion
    }
}
