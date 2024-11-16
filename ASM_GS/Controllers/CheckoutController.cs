using ASM_GS.Models;
using ASM_GS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ASM_GS.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display checkout page
        public async Task<IActionResult> Index()
        {
            var maKhachHang = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(maKhachHang))
            {
                return RedirectToAction("Index", "LoginAndSignUp");
            }

            var customer = await _context.KhachHangs
    .Where(kh => kh.MaKhachHang == maKhachHang)
    .Select(kh => new CheckoutViewModel
    {
        MaKhachHang = kh.MaKhachHang,
        TenKhachHang = kh.TenKhachHang,
        SoDienThoai = kh.SoDienThoai,
        DiaChi = kh.DiaChi,
        Email = _context.TaiKhoans
            .Where(tk => tk.MaKhachHang == maKhachHang)
            .Select(tk => tk.Email)
            .FirstOrDefault(),
        CartItems = _context.GioHangs
            .Where(g => g.MaKhachHang == maKhachHang)
            .SelectMany(g => g.ChiTietGioHangs)
            .Include(item => item.SanPham) // Liên kết trực tiếp với SanPham
            .Select(item => new CartItemViewModel
            {
                ItemId = item.Id,
                ProductId = item.MaSanPham,
                Quantity = item.SoLuong,
                Price = item.SanPham.Gia, // Lấy giá từ bảng SanPham
                ProductName = item.SanPham.TenSanPham,
                ImageUrl = item.SanPham.AnhSanPhams.FirstOrDefault() != null
                    ? item.SanPham.AnhSanPhams.FirstOrDefault().UrlAnh
                    : "/images/default-product.jpg"
            }).ToList()
    })
    .FirstOrDefaultAsync();


            if (customer == null || customer.CartItems.Count == 0)
            {
                ViewBag.NoItemsAlert = true;
                return View(customer);
            }

            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            var maKhachHang = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(maKhachHang))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Index", "Cart");
            }

            // Lấy lại CartItems từ cơ sở dữ liệu nếu không có dữ liệu trong model.CartItems
            if (model.CartItems == null || !model.CartItems.Any())
            {
                model.CartItems = await _context.GioHangs
                    .Where(g => g.MaKhachHang == maKhachHang)
                    .SelectMany(g => g.ChiTietGioHangs)
                    .Include(item => item.SanPham)
                    .Select(item => new CartItemViewModel
                    {
                        ItemId = item.Id,
                        ProductId = item.MaSanPham,
                        Quantity = item.SoLuong,
                        Price = item.SanPham.Gia,
                        ProductName = item.SanPham.TenSanPham,
                        ImageUrl = item.SanPham.AnhSanPhams.Any()
                            ? item.SanPham.AnhSanPhams.FirstOrDefault().UrlAnh
                            : "/images/default-product.jpg"
                    }).ToListAsync();
            }

            // Tính lại Total dựa trên CartItems trong POST
            decimal totalAmount = model.CartItems.Sum(i => i.Price * i.Quantity);

            // Tạo mã hóa đơn mới
            var lastInvoice = await _context.HoaDons.OrderByDescending(h => h.MaHoaDon).FirstOrDefaultAsync();
            string newInvoiceId = "HD" + ((lastInvoice != null ? int.Parse(lastInvoice.MaHoaDon.Substring(2)) : 0) + 1).ToString("D3");

            // Tạo mã đơn hàng mới
            var lastOrder = await _context.DonHangs.OrderByDescending(d => d.MaDonHang).FirstOrDefaultAsync();
            string newOrderId = "DH" + ((lastOrder != null ? int.Parse(lastOrder.MaDonHang.Substring(2)) : 0) + 1).ToString("D3");

            // Tạo hóa đơn và đơn hàng với `totalAmount`
            var hoaDon = new HoaDon
            {
                MaHoaDon = newInvoiceId,
                MaKhachHang = maKhachHang,
                NgayXuatHoaDon = DateOnly.FromDateTime(DateTime.Now),
                TongTien = totalAmount,
                TrangThai = model.PaymentMethod == "COD" ? 0 : 1
            };
            _context.HoaDons.Add(hoaDon);

            var donHang = new DonHang
            {
                MaDonHang = newOrderId,
                MaKhachHang = maKhachHang,
                NgayDatHang = DateOnly.FromDateTime(DateTime.Now),
                TrangThai = 0,
                TongTien = totalAmount
            };
            _context.DonHangs.Add(donHang);

            // Lưu chi tiết hóa đơn và đơn hàng
            foreach (var item in model.CartItems)
            {
                _context.ChiTietHoaDons.Add(new ChiTietHoaDon
                {
                    MaHoaDon = hoaDon.MaHoaDon,
                    MaSanPham = item.ProductId,
                    SoLuong = item.Quantity,
                    Gia = item.Price
                });

                _context.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaDonHang = donHang.MaDonHang,
                    MaSanPham = item.ProductId,
                    SoLuong = item.Quantity,
                    Gia = item.Price
                });
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đặt hàng thành công!";
            return RedirectToAction("OrderConfirmation", new { orderId = donHang.MaDonHang });
        }




        [HttpGet]
        public async Task<IActionResult> VNPayPaymentConfirmation(string orderId, bool success)
        {
            if (success)
            {
                var hoaDon = await _context.HoaDons.FirstOrDefaultAsync(h => h.MaHoaDon == orderId);
                if (hoaDon != null)
                {
                    hoaDon.TrangThai = 1; // Mark as paid
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Thanh toán VNPay thành công! Đơn hàng của bạn đang được xử lý.";
                    return RedirectToAction("OrderConfirmation", "Checkout", new { orderId = orderId });
                }
            }

            TempData["ErrorMessage"] = "Thanh toán VNPay không thành công. Vui lòng thử lại.";
            return RedirectToAction("Index", "Cart");
        }

        public IActionResult OrderConfirmation(string orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        private async Task ClearCart(string maKhachHang)
        {
            var gioHang = await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);

            if (gioHang != null)
            {
                _context.ChiTietGioHangs.RemoveRange(gioHang.ChiTietGioHangs);
                await _context.SaveChangesAsync();
            }
        }
    }
}