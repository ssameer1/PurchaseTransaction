namespace PurchaseTransaction.Utils
{
    public static class Money
    {
        public static decimal RoundToCents(decimal amount)
        {
            return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        }
    }
}
