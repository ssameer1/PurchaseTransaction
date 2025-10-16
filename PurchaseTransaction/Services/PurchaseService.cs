using Microsoft.EntityFrameworkCore;
using PurchaseTransaction.Data;
using PurchaseTransaction.Dto;
using PurchaseTransaction.Models;
using PurchaseTransaction.Utils;

namespace PurchaseTransaction.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly PTDbContext _dbContext;
        private readonly ILogger<PurchaseService> _logger;
        private readonly IExchangeRateService _exchangeRateService;

        public PurchaseService(PTDbContext pTDbContext, ILogger<PurchaseService> logger, IExchangeRateService exchangeRateService)
        {
            _dbContext = pTDbContext;
            _logger = logger;
            _exchangeRateService = exchangeRateService;
        }
        public async Task<PurchaseResponseDto> CreateAsync(CreatePurchaseRequestDto createPurchaseRequestDto)
        {
            try
            {
                if(createPurchaseRequestDto is null) throw new ArgumentNullException(nameof(createPurchaseRequestDto));

                var rounded = Money.RoundToCents(createPurchaseRequestDto.AmountUsd);
                var entity = new Purchase
                {
                     Description = createPurchaseRequestDto.Description,
                     TransactionDate = createPurchaseRequestDto.TransactionDate,
                     AmountUsd = rounded
                };

                _dbContext.Purchases.Add(entity);
                await _dbContext.SaveChangesAsync();

                return new PurchaseResponseDto
                {
                    Id = entity.Id,
                    Description = entity.Description,
                    TransactionDate = entity.TransactionDate,
                    OriginalUsdAmount = entity.AmountUsd
                };

            }
            catch(ArgumentNullException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase");
                throw;
            }
        }

        public async Task<PurchaseResponseDto?> GetAsync(Guid id)
        {
            try
            {
                var purch = await _dbContext.Purchases.FirstOrDefaultAsync(s=> s.Id == id);
                if (purch == null) return null;

                return new PurchaseResponseDto
                {
                    Id = purch.Id,
                    Description = purch.Description,
                    TransactionDate = purch.TransactionDate,
                    OriginalUsdAmount = purch.AmountUsd
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting purchase by id {Id}", id);
                throw;
            }
        }

        public async Task<PurchaseResponseDto?> GetConvertedAsync(Guid id, string targetCurrencyCode)
        {
            try
            {
                var purch = await _dbContext.Purchases.FirstOrDefaultAsync(s => s.Id == id);
                if (purch == null) return null;
                var rate = await _exchangeRateService.GetRateOnOrBeforeAsync(targetCurrencyCode, purch.TransactionDate);
                if (rate == null) throw new InvalidOperationException($"No exchange rate found for {targetCurrencyCode} within 6 months or before the purchase date");
                var converted = Money.RoundToCents(purch.AmountUsd * rate.RatePerUsd);

                return new PurchaseResponseDto
                {
                    Id = purch.Id,
                    Description = purch.Description,
                    TransactionDate = purch.TransactionDate,
                    OriginalUsdAmount = purch.AmountUsd,
                    TargetCurrency = targetCurrencyCode,
                    ExchangeRateUsed = rate.RatePerUsd,
                    ConvertedAmount = converted,
                    ExchangeRateDate = rate.RateDate
                };

            }
            catch (InvalidOperationException) { throw; }
            catch (ArgumentException) { throw;  }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting converted purchase by id {Id} to {TargetCurrency}", id, targetCurrencyCode);
                throw;
            }
        }
    }
}
