namespace PurchaseTransaction.Models
{
    public class FxRate
    {
        public DateOnly RateDate { get; set; }
        public decimal RatePerUsd { get; set; }
    }
}
