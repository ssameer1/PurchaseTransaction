using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PurchaseTransaction.Data;
using PurchaseTransaction.Dto;
using PurchaseTransaction.Models;
using PurchaseTransaction.Services;

namespace PurchaseTransaction.Test
{
    public class PurchaseServiceTest
    {
        private static PTDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<PTDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var ctx = new PTDbContext(options);
            ctx.Database.EnsureCreated();
            return new PTDbContext(options);
        }

        [Test]
        public async Task CreateAsync_presists_and_rounds_to_cents()
        {
            using var context = CreateContext(nameof(CreateAsync_presists_and_rounds_to_cents));

            var exchangeRateService = new Mock<IExchangeRateService>(MockBehavior.Strict);
            var logger = NullLogger<PurchaseService>.Instance;
            var service = new PurchaseService(context, logger, exchangeRateService.Object);
            var createDto = new CreatePurchaseRequestDto
            {
                Description = "Test Purchase",
                TransactionDate = new DateOnly(2025, 9, 18),
                AmountUsd = 10.4995m
            };

            var result = await service.CreateAsync(createDto);

            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(result.Description, Is.EqualTo(createDto.Description));
                Assert.That(result.TransactionDate, Is.EqualTo(createDto.TransactionDate));
                Assert.That(result.OriginalUsdAmount, Is.EqualTo(10.50m)); // Rounded to cents
            });

            var entityInDb = await context.Purchases.FirstOrDefaultAsync(p => p.Id == result.Id);
            Assert.That(entityInDb, Is.Not.Null);
            Assert.That(entityInDb!.AmountUsd, Is.EqualTo(10.50m));
        }

        [Test]
        public async Task GetAsync_returns_null_when_not_found()
        {
            using var context = CreateContext(nameof(GetAsync_returns_null_when_not_found));
            var exchangeRateService = new Mock<IExchangeRateService>(MockBehavior.Strict);
            var logger = NullLogger<PurchaseService>.Instance;
            var service = new PurchaseService(context, logger, exchangeRateService.Object);

            var result = await service.GetAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetConvertedAsync_throws_InvalidOperationException_when_no_rate_within_window()
        {
            using var context = CreateContext(nameof(GetConvertedAsync_throws_InvalidOperationException_when_no_rate_within_window));
            var purchase = new Purchase
            {
                Id = Guid.NewGuid(),
                Description = "Test Purchase",
                TransactionDate = new DateOnly(2025, 9, 18),
                AmountUsd = 20.00m
            };
            context.Purchases.Add(purchase);
            context.SaveChanges();

            var exchangeRateService = new Mock<IExchangeRateService>();
                exchangeRateService.Setup(s => s.GetRateOnOrBeforeAsync("Canada-Dollar", purchase.TransactionDate)).ReturnsAsync((FxRate?)null);

            var logger = NullLogger<PurchaseService>.Instance;
            var service = new PurchaseService(context, logger, exchangeRateService.Object);
            
           
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.GetConvertedAsync(purchase.Id, "Canada-Dollar"));
            Assert.That(ex!.Message, Does.Contain("No exchange rate found for Canada-Dollar within 6 months or before the purchase date"));
        }
    }
}
