using PurchaseTransaction.Dto;

namespace PurchaseTransaction.Services
{
    public interface IPurchaseService
    {
        Task<PurchaseResponseDto> CreateAsync(CreatePurchaseRequestDto createPurchaseRequestDto);
        Task<PurchaseResponseDto?> GetAsync(Guid id);
        Task<PurchaseResponseDto?> GetConvertedAsync(Guid id, string targetCurrencyCode);
    }
}
