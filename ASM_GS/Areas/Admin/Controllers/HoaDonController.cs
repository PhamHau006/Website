using ASM_GS.Controllers;
using ASM_GS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using X.PagedList;
using X.PagedList.Extensions;

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

        // Hiển thị danh sách hóa đơn với phân trang
        [HttpGet]
        public IActionResult Index(string? search, int? trangThai, int? page, int pageSize = 10)
        {
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
            hoaDons = hoaDons.OrderByDescending(hd => hd.NgayXuatHoaDon);

            // Phân trang
            int pageNumber = page ?? 1;
            var pagedHoaDons = hoaDons.ToPagedList(pageNumber, pageSize);

            // Truyền dữ liệu cho View
            ViewBag.Search = search;
            ViewBag.PageSize = pageSize;
            ViewData["CurrentTrangThai"] = trangThai;
            ViewBag.TrangThai = trangThai; // Truyền giá trị trạng thái vào ViewBag

            return View(pagedHoaDons); // Trả về IPagedList
        }

        // API lấy chi tiết hóa đơn
        [HttpGet]
        public IActionResult GetChiTietHoaDon(string maHoaDon)
        {
            var chiTiet = _context.ChiTietHoaDons
                .Where(ct => ct.MaHoaDon == maHoaDon)
                .Select(ct => new
                {
                    sanPham = ct.SanPham == null ? null : new
                    {
                        TenSanPham = ct.SanPham.TenSanPham,
                        HinhAnh = ct.SanPham.AnhSanPhams.Any() // Kiểm tra nếu có ảnh
                            ? ct.SanPham.AnhSanPhams.FirstOrDefault().UrlAnh // Lấy ảnh đầu tiên
                            : "/images/default-image.jpg" // Ảnh mặc định nếu không có
                    },
                    combo = ct.Combo == null ? null : new
                    {
                        TenCombo = ct.Combo.TenCombo,
                        HinhAnh = ct.Combo.Anh ?? "/images/default-image.jpg"
                    },
                    ct.SoLuong,
                    Gia = ct.SanPham != null ? ct.SanPham.Gia : 0,
                    ThanhTien = ct.SanPham != null ? ct.SanPham.Gia * ct.SoLuong : 0
                })
                .ToList();

            return Json(chiTiet);
        }



        // API xóa hóa đơn
        [HttpPost]
        public IActionResult Delete(string maHoaDon)
        {
            var hoaDon = _context.HoaDons.FirstOrDefault(hd => hd.MaHoaDon == maHoaDon);
            if (hoaDon == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
            }

            _context.HoaDons.Remove(hoaDon);
            _context.SaveChanges();

            return Json(new { success = true, message = "Đã xóa hóa đơn thành công" });
        }

        // API cập nhật trạng thái hóa đơn
        [HttpPost]
        public IActionResult UpdateTrangThai(string maHoaDon, int trangThai)
        {
            var hoaDon = _context.HoaDons.FirstOrDefault(hd => hd.MaHoaDon == maHoaDon);
            if (hoaDon == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
            }

            hoaDon.TrangThai = trangThai;
            _context.SaveChanges();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
        }
        // API cập nhật trạng thái hóa đơn (chỉ cập nhật cột Trạng thái)
        [HttpPost]
        public IActionResult CapNhatTrangThai(string maHoaDon, int trangThai)
        {
            var hoaDon = _context.HoaDons.FirstOrDefault(hd => hd.MaHoaDon == maHoaDon);
            if (hoaDon == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hóa đơn." });
            }

            hoaDon.TrangThai = trangThai;
            _context.SaveChanges();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
        }
    }
}