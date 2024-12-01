using ASM_GS.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using X.PagedList;
using X.PagedList.Extensions;
using System.IO;
using ASM_GS.Controllers;

namespace ASM_GS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HoaDonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HoaDonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Index - Hiển thị danh sách hóa đơn với tìm kiếm, phân trang và sắp xếp
        // Index - Hiển thị danh sách hóa đơn với tìm kiếm, phân trang và sắp xếp
        public IActionResult Index(string? search, int? trangThai, int? page, int pageSize = 10, string sortOrder = "Descending")
        {
            ViewBag.SortOrder = sortOrder;

            var hoaDons = _context.HoaDons.Include(hd => hd.KhachHang).AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                hoaDons = hoaDons.Where(hd => hd.MaHoaDon.Contains(search) || hd.MaKhachHang.Contains(search));
            }

            // Lọc trạng thái
            if (trangThai.HasValue)
            {
                hoaDons = hoaDons.Where(hd => hd.TrangThai == trangThai);
            }

            // Sắp xếp theo ngày xuất hóa đơn
            if (sortOrder == "Descending")
            {
                hoaDons = hoaDons.OrderByDescending(hd => hd.NgayXuatHoaDon);
            }
            else
            {
                hoaDons = hoaDons.OrderBy(hd => hd.NgayXuatHoaDon);
            }

            // Phân trang
            int pageNumber = page ?? 1;
            var pagedHoaDons = hoaDons.ToPagedList(pageNumber, pageSize);

            ViewBag.Search = search;
            ViewBag.PageSize = pageSize;
            ViewData["CurrentTrangThai"] = trangThai;

            // Kiểm tra nếu không có kết quả tìm kiếm
            if (!hoaDons.Any() && !string.IsNullOrEmpty(search))
            {
                TempData["SearchMessage"] = "Không tìm thấy kết quả với từ khóa: " + search;
            }

            return View(pagedHoaDons); // Trả về IPagedList
        }


        // GetChiTietHoaDon - Lấy chi tiết hóa đơn theo mã hóa đơn
        [HttpGet]
        public IActionResult GetChiTietHoaDon(string maHoaDon)
        {
            var hoaDon = _context.HoaDons.Include(h => h.ChiTietHoaDons)
                                         .ThenInclude(ct => ct.SanPham)
                                         .FirstOrDefault(h => h.MaHoaDon == maHoaDon);
            if (hoaDon == null)
            {
                return NotFound();
            }

            var chiTietHtml = hoaDon.ChiTietHoaDons.Select(ct => new
            {
                ct.SanPham.TenSanPham,
                ct.SanPham.AnhSanPhams,
                ct.SoLuong,
                ct.Gia,
                ThanhTien = ct.SoLuong * ct.Gia
            }).ToList();

            var tongTien = chiTietHtml.Sum(x => x.ThanhTien);
            return Json(new { chiTietHtml, tongTien });
        }

        // ChangeStatus - Cập nhật trạng thái của hóa đơn
        [HttpPost]
        public IActionResult ChangeStatus(string maHoaDon, int trangThai)
        {
            var hoaDon = _context.HoaDons.FirstOrDefault(h => h.MaHoaDon == maHoaDon);
            if (hoaDon != null)
            {
                hoaDon.TrangThai = trangThai == 1 ? 0 : 1; // Đổi trạng thái
                _context.SaveChanges();
            }
            return NoContent();
        }

        // Enum trạng thái hóa đơn
        public enum TrangThaiHoaDon
        {
            ChuaThanhToan = 0,
            DaThanhToan = 1
        }

        // ExportAllHoaDons - Xuất toàn bộ hóa đơn ra file Word
        public IActionResult ExportAllHoaDons()
        {
            var hoaDons = _context.HoaDons.ToList();
            var fileBytes = ExportHoaDonsToWord(hoaDons);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "ToanBoHoaDon.docx");
        }

        // ExportHoaDonsToWord - Hàm xuất hóa đơn ra file Word
        private byte[] ExportHoaDonsToWord(List<HoaDon> hoaDons)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Sử dụng thư viện Open XML SDK để tạo file Word
                using (var document = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
                {
                    var mainPart = document.AddMainDocumentPart();
                    mainPart.Document = new Document(new Body());

                    // Thêm tiêu đề cho bảng
                    var table = new Table();
                    table.AppendChild(new TableRow(
                        new TableCell(new Paragraph(new Run(new Text("Mã hóa đơn")))),
                        new TableCell(new Paragraph(new Run(new Text("Mã khách hàng")))),
                        new TableCell(new Paragraph(new Run(new Text("Ngày xuất")))),
                        new TableCell(new Paragraph(new Run(new Text("Tổng tiền")))),
                        new TableCell(new Paragraph(new Run(new Text("Trạng thái"))))
                    ));

                    // Thêm dữ liệu hóa đơn
                    foreach (var hoaDon in hoaDons)
                    {
                        table.AppendChild(new TableRow(
                            new TableCell(new Paragraph(new Run(new Text(hoaDon.MaHoaDon)))),
                            new TableCell(new Paragraph(new Run(new Text(hoaDon.MaKhachHang)))),
                            new TableCell(new Paragraph(new Run(new Text(hoaDon.NgayXuatHoaDon.ToShortDateString())))),
                            new TableCell(new Paragraph(new Run(new Text(hoaDon.TongTien.ToString("N0") + " VNĐ")))),
                            new TableCell(new Paragraph(new Run(new Text(hoaDon.TrangThai == 1 ? "Đã thanh toán" : "Chưa thanh toán"))))
                        ));
                    }

                    // Thêm bảng vào body của document
                    mainPart.Document.Body.Append(table);
                    mainPart.Document.Save();
                }

                return memoryStream.ToArray();
            }
        }
    }
}
