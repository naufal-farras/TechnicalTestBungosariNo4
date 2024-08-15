using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using TechnicalTestBungosariNo4.Models;

namespace TechnicalTestBungosariNo4.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly InventoryContext _context;
        public HomeController(ILogger<HomeController> logger, InventoryContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetTable()
        {
            var result = _context.T_Transaksi.Include(x => x.inventoryItem).ToList().GroupBy(x => x.inventoryItem.Id).ToList();
            return Ok(new { data = result });
        }
        public IActionResult GetById(int Id)
        {
            try
            {
                var result = _context.T_Transaksi.Where(x => x.Id == Id).FirstOrDefault();
                return Ok(result);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null)
                {
                    msg = e.InnerException.Message;
                }
                return BadRequest(msg);
            }

        }

        public IActionResult PostTransaksi(Transaksi Transaksi, DateTime inoutdate, int Type)
        {
            try
            {
                if (Transaksi.Id == 0)
                {
                    _context.T_Transaksi.Add(Transaksi);
                }
                else
                {
                    var data = _context.T_Transaksi
                        .Where(x => x.Id == Transaksi.Id)
                        .FirstOrDefault();
                    data.QTY = Transaksi.QTY;
                    data.Type = Transaksi.Type;
                    data.inOutBoundDate = inoutdate;
                    data.UpdateDate = DateTime.Now;
                    _context.Entry(data).State = EntityState.Modified;
                }
                _context.SaveChanges();
                return Ok(Transaksi);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null)
                {
                    msg = e.InnerException.Message;
                }
                return BadRequest(msg);
            }
        }
        public IActionResult PostProduk(Produk Items)
        {
            try
            {
                if (Items.Id == 0)
                {
                    Produk Item = new Produk();
                    Item.NamaProduk = Items.NamaProduk;
                    Item.Harga = Items.Harga;
                    _context.M_Produk.Add(Item);
                    _context.SaveChanges();

                    Transaksi transaksi = new Transaksi();
                    transaksi.QTY = 0;
                    transaksi.Type = 3; // new product
                    transaksi.inOutBoundDate = DateTime.Now;
                    transaksi.inventoryItemId = Item.Id;
                    _context.T_Transaksi.Add(transaksi);
                    _context.SaveChanges();
                }
                else
                {
                    var data = _context.M_Produk
                        .Where(x => x.Id == Items.Id)
                        .FirstOrDefault();
                    data.Harga = Items.Harga;
                    data.NamaProduk = Items.NamaProduk;
                    _context.Entry(data).State = EntityState.Modified;
                    _context.SaveChanges();

                }
                return Ok(Items);
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null)
                {
                    msg = e.InnerException.Message;
                }
                return BadRequest(msg);
            }
        }

        public IActionResult Delete(int id)
        {
            try
            {
                var result = 0;
                var data = _context.T_Transaksi.Where(x => x.Id == id).FirstOrDefault();
                if (data != null)
                {
                    data.isDeleted = true;
                    _context.Entry(data).State = EntityState.Modified;
                    _context.SaveChanges();
                    return Ok(data);
                }
                else
                {
                    throw new Exception("Data Tidak Ditemukan!");
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null)
                {
                    msg = e.InnerException.Message;
                }
                return BadRequest(msg);
            }

        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
