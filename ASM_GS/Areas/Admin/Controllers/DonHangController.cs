using ASM_GS.Controllers;
using ASM_GS.Models;
using ASM_GS.ViewModels;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ASM_GS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DonHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action hiển thị danh sách đơn hàng
        public async Task<IActionResult> Index(string trangThai = "all")
        {
            if (HttpContext.Session.GetString("StaffAccount") == null)
            {
                HttpContext.Session.SetString("RedirectUrl", HttpContext.Request.GetDisplayUrl());
                ViewData["RedirectUrl"] = HttpContext.Session.GetString("RedirectUrl");
            }
            IQueryable<DonHang> ordersQuery = _context.DonHangs
                .Include(dh => dh.ChiTietDonHangs)
                .ThenInclude(ct => ct.MaSanPhamNavigation);

            switch (trangThai.ToLower())
            {
                case "processing":
                    ordersQuery = ordersQuery.Where(dh => dh.TrangThai == 0);
                    break;
                case "shipped":
                    ordersQuery = ordersQuery.Where(dh => dh.TrangThai == 1);
                    break;
                case "completed":
                    ordersQuery = ordersQuery.Where(dh => dh.TrangThai == 2);
                    break;
                case "refunded":
                    ordersQuery = ordersQuery.Where(dh => dh.TrangThai == 4);  // Hiển thị đơn hàng đã hoàn trả
                    break;
                case "all":
                default:
                    ordersQuery = ordersQuery;
                    break;
            }

            var orders = await ordersQuery.Select(dh => new DonHangViewModel
            {
                MaDonHang = dh.MaDonHang,
                MaKhachHang = dh.MaKhachHang,
                NgayDatHang = dh.NgayDatHang,
                TongTien = dh.TongTien,
                TrangThai = dh.TrangThai ?? 0,
                ChiTietDonHangs = dh.ChiTietDonHangs.Select(ct => new ChiTietDonHangViewModel
                {
                    MaSanPham = ct.MaSanPham,
                    SoLuong = ct.SoLuong,
                    Gia = ct.Gia,
                    TenSanPham = ct.MaSanPhamNavigation != null ? ct.MaSanPhamNavigation.TenSanPham : "Sản phẩm không xác định",
                    UrlAnhSanPham = ct.MaSanPhamNavigation != null && ct.MaSanPhamNavigation.AnhSanPhams.FirstOrDefault() != null
                        ? ct.MaSanPhamNavigation.AnhSanPhams.FirstOrDefault().UrlAnh
                        : "/images/default-product.jpg"
                }).ToList()
            }).ToListAsync();

            return View(orders);
        }

        // Hành động duyệt đơn và thay đổi trạng thái
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(string maDonHang, int trangThai)
        {
            var order = await _context.DonHangs
                .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);

            if (order != null)
            {
                order.TrangThai = trangThai;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Trạng thái đơn hàng đã được cập nhật thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
            }

            return RedirectToAction("Index");
        }

        // Hành động hoàn trả đơn hàng
        [HttpPost]
        public async Task<IActionResult> Refund(string maDonHang)
        {
            var order = await _context.DonHangs
                .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);

            if (order != null)
            {
                if (order.TrangThai == 2)  // Kiểm tra trạng thái "Đã giao"
                {
                    order.TrangThai = 4;  // Đặt trạng thái "Hoàn trả"
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Đơn hàng đã được hoàn trả thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Đơn hàng không thể hoàn trả vì chưa được giao!";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
            }

            return RedirectToAction("Index");
        }

        // Hành động lấy chi tiết đơn hàng
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(string maDonHang)
        {
            var order = await _context.DonHangs
                .Include(dh => dh.ChiTietDonHangs)
                .ThenInclude(ct => ct.MaSanPhamNavigation)
                .FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);

            if (order == null)
            {
                return NotFound();
            }

            // Trả về một Partial View để hiển thị chi tiết
            return PartialView("_OrderDetails", order);
        }
    }
}
