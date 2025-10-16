using Microsoft.EntityFrameworkCore;
using PurchaseTransaction.Models;

namespace PurchaseTransaction.Data
{
    public class PTDbContext : DbContext
    {
        public PTDbContext(DbContextOptions<PTDbContext> options) : base(options)
        {
        }
        public DbSet<Purchase> Purchases => Set<Purchase>();
    }
}
