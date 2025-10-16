using PurchaseTransaction.Models;

namespace PurchaseTransaction.Services
{
    public partial class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExchangeRateService> _logger;
        public ExchangeRateService(HttpClient httpClient, ILogger<ExchangeRateService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<FxRate?> GetRateOnOrBeforeAsync(string currencyCode, DateOnly purchaseDate)
        {

            var start = purchaseDate.AddMonths(-6);
            var end = purchaseDate;

            var path = "services/api/fiscal_service/v1/accounting/od/rates_of_exchange";
            var url = $"{path}?fields=record_date,currency,exchange_rate&filter=country_currency_desc:eq:{currencyCode},record_date:gte:{start:yyyy-MM-dd},record_date:lte:{end:yyyy-MM-dd}&sort=-record_date&page[size]=1";

            try
            {
                var resp = await _httpClient.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get exchange rate for {CurrencyCode} on or before {PurchaseDate}. Status code: {StatusCode}", currencyCode, purchaseDate, resp.StatusCode);
                    return null;
                }

                var data = await resp.Content.ReadFromJsonAsync<TreasuryResponse>();
                var item = data?.Data?.FirstOrDefault();
                if (item == null)
                {
                    _logger.LogInformation("No exchange rate found for {CurrencyCode} on or before {PurchaseDate}", currencyCode, purchaseDate);
                    return null;
                }

                if (!DateOnly.TryParse(item.Record_Date.ToString(), out var rateDate))
                {
                    _logger.LogWarning("Failed to parse rate date {RateDate}", item.Record_Date);
                    return null;
                }
                if (!decimal.TryParse(item.Exchange_Rate, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var ratePerUsd))
                {
                    _logger.LogWarning("Failed to parse exchange rate {ExchangeRate}", item.Exchange_Rate);
                    return null;
                }
                return new FxRate { RateDate = rateDate, RatePerUsd = ratePerUsd };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception getting exchange rate for {CurrencyCode} on or before {PurchaseDate}", currencyCode, purchaseDate);
                return null;
            }
        }
    }
}
