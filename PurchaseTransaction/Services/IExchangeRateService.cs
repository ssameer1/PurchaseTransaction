using PurchaseTransaction.Models;
using System.Threading.RateLimiting;

namespace PurchaseTransaction.Services
{

    public interface IExchangeRateService
    {
        Task<FxRate?> GetRateOnOrBeforeAsync(string currencyCode, DateOnly purchaseDate);
    }
}
