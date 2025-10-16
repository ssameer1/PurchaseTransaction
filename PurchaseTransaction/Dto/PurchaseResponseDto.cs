namespace PurchaseTransaction.Dto
{
    public class PurchaseResponseDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateOnly TransactionDate { get; set; }
        public decimal OriginalUsdAmount { get; set; }
        public string? TargetCurrency { get; set; }
        public decimal? ExchangeRateUsed { get; set; }
        public decimal? ConvertedAmount { get; set; }
        public DateOnly? ExchangeRateDate { get; set; }
    }
}
