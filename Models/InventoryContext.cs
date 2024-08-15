using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


namespace TechnicalTestBungosariNo4.Models
{
    public class InventoryContext : DbContext
    {
        public InventoryContext(DbContextOptions<InventoryContext> options)
            : base(options)
        {
        }

        public DbSet<Produk> M_Produk { get; set; }
        public DbSet<Transaksi> T_Transaksi { get; set; }
    }
}
