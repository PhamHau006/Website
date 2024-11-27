using ASM_GS.Controllers;
using ASM_GS.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM_GS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Get session data
            var maTaiKhoan = HttpContext.Session.GetString("StaffAccount");
            var profilePicture = HttpContext.Session.GetString("ProfilePicture") ?? "/assets/assets/img/avatars/default.png";

            if (string.IsNullOrEmpty(maTaiKhoan))
            {
                HttpContext.Session.SetString("RedirectUrl", HttpContext.Request.GetDisplayUrl());
                ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
            }

            var userName = HttpContext.Session.GetString("StaffName");
            var userRole = HttpContext.Session.GetString("StaffRole");

            // Pass session values to view
            ViewBag.UserName = userName;
            ViewBag.UserRole = userRole;
            ViewBag.ProfilePicture = profilePicture;

            // Fetch data for statistics
            // Revenue by product
            var doanhThuSanPham = _context.ChiTietHoaDons
            
            .Include(c => c.HoaDon)
            .Include(c => c.SanPham)
             .Where(c => c.HoaDon.TrangThai == 1)
            .GroupBy(c => c.MaSanPham)
            .Select(g => new
            {
                TenSanPham = g.FirstOrDefault() != null && g.FirstOrDefault().SanPham != null
                    ? g.FirstOrDefault().SanPham.TenSanPham
                    : "Combo",
                TongDoanhThu = g.Sum(c => c.SoLuong * c.Gia),
                TongSoLuong = g.Sum(c => c.SoLuong)
            })
            .OrderByDescending(x => x.TongDoanhThu)
            .ToList();

            //Revenue by combos
            var doanhThuCombo = _context.ChiTietHoaDons
                .Include (c => c.HoaDon)
                .Include (c => c.Combo)
                 .Where(c => c.HoaDon.TrangThai == 1)
                .GroupBy (c => c.MaCombo)
                .Select(g => new
                {
                    TenCombo = g.FirstOrDefault() != null && g.FirstOrDefault().Combo != null
                    ? g.FirstOrDefault().Combo.TenCombo : "Sản phẩm",
                    TongDoanhThu = g.Sum (c => c.SoLuong * c.Gia),
                    TongSoLuong = g.Sum(c => c.SoLuong)
                })
                .OrderByDescending (x => x.TongDoanhThu)
                .ToList();
            // Revenue by day
            var doanhThuTheoNgay = _context.HoaDons
                .Where(h => h.TrangThai == 1)
                .GroupBy(h => h.NgayXuatHoaDon)
                .Select(g => new
                {
                    Ngay = g.Key,
                    TongDoanhThu = g.Sum(h => h.TongTien),
                    SoHoaDon = g.Count()
                }).ToList();

            // Revenue by month
            var doanhThuTheoThang = _context.HoaDons
                .Where(h => h.TrangThai == 1)
                .GroupBy(h => new { h.NgayXuatHoaDon.Year, h.NgayXuatHoaDon.Month })
                .Select(g => new
                {
                    Thang = $"{g.Key.Month}/{g.Key.Year}",
                    TongDoanhThu = g.Sum(h => h.TongTien),
                    SoHoaDon = g.Count()
                }).ToList();

            // Revenue by year
            var doanhThuTheoNam = _context.HoaDons
                .Where(h => h.TrangThai == 1)
                .GroupBy(h => h.NgayXuatHoaDon.Year)
                .Select(g => new
                {
                    Nam = g.Key,
                    TongDoanhThu = g.Sum(h => h.TongTien),
                    SoHoaDon = g.Count()
                }).ToList();
            // Tổng doanh thu của tất cả hóa đơn
            var tongDoanhThuTatCaHoaDon = _context.HoaDons
                .Where(h => h.TrangThai == 1) // Chỉ tính các hóa đơn đã hoàn thành (hoặc trạng thái hợp lệ)
                .Sum(h => h.TongTien);

            // Pass data to the view using ViewBag
            ViewBag.DoanhThuSanPham = doanhThuSanPham;
            ViewBag.DoanhThuTheoNgay = doanhThuTheoNgay;
            ViewBag.DoanhThuTheoThang = doanhThuTheoThang;
            ViewBag.DoanhThuTheoNam = doanhThuTheoNam;
            ViewBag.DoanhThuCombo = doanhThuCombo;
            ViewBag.TongDoanhThuTatCaHoaDon = tongDoanhThuTatCaHoaDon;


            return View();
        }

        [HttpPost]
        public IActionResult RemoveRoutedFromLoginSession()
        {
            HttpContext.Session.Remove("RedirectUrl");
            ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
            return Ok();
        }

        [HttpPost]
        public IActionResult RemoveStaffAccount()
        {
            HttpContext.Session.Remove("StaffAccount");
            HttpContext.Session.Remove("Staff");
            return RedirectToAction("Index", "LoginAdmin", new { area = "" });
        }
    }
}
