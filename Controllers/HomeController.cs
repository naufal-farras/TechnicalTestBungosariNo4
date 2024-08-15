using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.Pdf;
using System;
using Syncfusion.DocIORenderer;
using System.Diagnostics;
using TechnicalTestBungosariNo4.Models;

namespace TechnicalTestBungosariNo4.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly InventoryContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, InventoryContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetTable()
        {
            var result = _context.M_Produk
                .Where(p => !p.isDeleted) 
                .GroupJoin(
                    _context.T_Transaksi.Where(t => !t.isDeleted), 
                    p => p.Id,                                     
                    t => t.inventoryItemId,                        
                    (p, transactions) => new
                    {
                        NamaProduk = p.NamaProduk,
                        Harga = p.Harga,
                        Inbound = transactions.Where(x=>x.Type == 1).ToList().Count(),
                        Outbond = transactions.Where(x=>x.Type == 2).ToList().Count(),
                    }
                )
                .ToList();
            return Ok(new { data = result });
        }
        public IActionResult GetById(int Id)
        {
            try
            {
                var result = _context.T_Transaksi.Include(x=>x.inventoryItem).Where(x => x.Id == Id).FirstOrDefault();
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

        public IActionResult PostTransaksi(Transaksi Transaksi)
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
                    data.inOutBoundDate = Transaksi.inOutBoundDate;
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

        //Generate Report
        public IActionResult GenerateReport(DateTime dateStart, DateTime dateEnd)
        {
            try
            {
                var trans = _context.T_Transaksi
                    .Include(x => x.inventoryItem)
                    .Where(x => x.isDeleted == false && x.inOutBoundDate >= dateStart && x.inOutBoundDate < dateEnd.AddDays(1))
                    .ToList();

                string webRootPath = _webHostEnvironment.WebRootPath;
                string filename = "TemplateHISTORYINVETORY.docx";

                FileStream fileStreamPath = new FileStream(Path.Combine(webRootPath, filename), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                WordDocument document = new WordDocument(fileStreamPath, FormatType.Docx);
                //Table
                IWTextRange textRange;
                WSection section = document.Sections[0];
                WTable table = section.Tables[0] as WTable;
                table.TableFormat.HorizontalAlignment = RowAlignment.Center;

                WTableRow row;
                WTableCell cell;

                var no = 1;
                foreach (var item in trans)
                {
                    row = table.AddRow(true, false);
                    row.Height = 18;
                    row.HeightType = TableRowHeightType.AtLeast;

                    // Cell : No
                    cell = row.AddCell();
                    cell.AddParagraph().AppendText(no.ToString()).CharacterFormat.FontSize = 8;
                    cell.CellFormat.VerticalAlignment = VerticalAlignment.Middle;
                    no++;

                    //Cell: Nama Produk
                    cell = row.AddCell();
                    textRange = cell.AddParagraph().AppendText(item.inventoryItem.NamaProduk?.ToString() ?? "-");
                    textRange.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
                    textRange.CharacterFormat.FontName = "Calibri";
                    textRange.CharacterFormat.FontSize = 8;
                    cell.CellFormat.VerticalAlignment = VerticalAlignment.Middle;

                    //Cell : Type Transaksi
                    string types = "";
                    if (item.Type == 1)
                    {
                        types = "INBOUND";
                    }
                    else if (item.Type == 2)
                    {
                        types = "OUTBOND";
                    }
                    else if (item.Type == 3)
                    {
                        types = "NEW";
                    }
                    else
                    {
                        types = "";
                    }
                    cell = row.AddCell();
                    textRange = cell.AddParagraph().AppendText(types);
                    textRange.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
                    textRange.CharacterFormat.FontName = "Calibri";
                    textRange.CharacterFormat.FontSize = 8;
                    cell.CellFormat.VerticalAlignment = VerticalAlignment.Middle;

                    //Cell : Quantity Stok
                    cell = row.AddCell();
                    textRange = cell.AddParagraph().AppendText(item.QTY.ToString());
                    textRange.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
                    textRange.CharacterFormat.FontName = "Calibri";
                    textRange.CharacterFormat.FontSize = 8;
                    cell.CellFormat.VerticalAlignment = VerticalAlignment.Middle;

                    //Cell : Harga
                    cell = row.AddCell();
                    textRange = cell.AddParagraph().AppendText(item.inventoryItem.Harga.ToString("#,##0"));
                    textRange.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
                    textRange.CharacterFormat.FontName = "Calibri";
                    textRange.CharacterFormat.FontSize = 8;
                    cell.CellFormat.VerticalAlignment = VerticalAlignment.Middle;

                    //Cell : Trans Date
                    cell = row.AddCell();
                    textRange = cell.AddParagraph().AppendText(item.inOutBoundDate.ToString("yyyyMMdd"));
                    textRange.OwnerParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
                    textRange.CharacterFormat.FontName = "Calibri";
                    textRange.CharacterFormat.FontSize = 8;
                    cell.CellFormat.VerticalAlignment = VerticalAlignment.Middle;

                }

                document.Replace("@@Tanggal@@", DateTime.Now.Day.ToString(), false, true);
                DocIORenderer render = new DocIORenderer();
                //render.Settings.ChartRenderingOptions.ImageFormat = ExportImageFormat.Jpeg;
                PdfDocument pdfDoc = render.ConvertToPDF(document);

                MemoryStream stream = new MemoryStream();

                pdfDoc.Save(stream);
                stream.Position = 0;
                pdfDoc.Close(true);
                document.Close();

                string contentType = "application/pdf";
                string filenamed = "Report_Inbound_Outbond" + dateStart.ToString("ddMMMMyyyy", new System.Globalization.CultureInfo("id-ID")) + "-" + dateEnd.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("id-ID")) + ".pdf";
                return File(stream, contentType, filenamed);

            }
            catch (Exception err)
            {

                return BadRequest(err.Message);
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
