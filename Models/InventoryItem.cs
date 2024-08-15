namespace TechnicalTestBungosariNo4.Models
{
    //[M_Produk]
    public class Produk
    {
        public int Id { get; set; }
        public string NamaProduk { get; set; }
        public decimal Harga { get; set; }
        public bool isDeleted { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime? UpdateDate { get; set; }

    }
    //[T_Transaksi]
    public class Transaksi
    {
        public int Id { get; set; }
        public int QTY { get; set; }
        public int Type { get; set; }
        public Produk inventoryItem { get; set; }
        public int inventoryItemId { get; set; }
        public DateTime inOutBoundDate { get; set; }
        public bool isDeleted { get; set; }

        public DateTime CreateDate{ get; set; } = DateTime.Now;
        public DateTime? UpdateDate { get; set; } 
    }

}
