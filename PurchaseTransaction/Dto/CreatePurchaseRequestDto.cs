using System.ComponentModel.DataAnnotations;

namespace PurchaseTransaction.Dto
{
    public class CreatePurchaseRequestDto
    {
        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(50, ErrorMessage = "Description cannot exceed 50 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "TransactionDate is required.")]
        [DataType(DataType.Date, ErrorMessage = "TransactionDate must be a valid date.")]
        public DateOnly TransactionDate { get; set; }

        [Required(ErrorMessage = "Purchase amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Purchase amount must be between 0.01 and 1,000,000.")]
        public decimal AmountUsd { get; set; }
    }
}
