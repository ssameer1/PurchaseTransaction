using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PurchaseTransaction.Models
{
    public class Purchase
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(50)]
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public DateOnly TransactionDate { get; set; }
        [Precision(18,2)]
        [Required]
        public decimal AmountUsd { get; set; }
        [Required]
        public DateTime CreateUtc { get; set; } = DateTime.UtcNow;
    }
}
