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
                        .Select(item => new CartItemViewModel
                        {
                            ItemId = item.Id,
                            ProductId = item.MaSanPham,
                            Quantity = item.SoLuong,
                            Price = _context.SanPhams
                                .Where(p => p.MaSanPham == item.MaSanPham)
                                .Select(p => (decimal?)p.Gia)
                                .FirstOrDefault() ?? 0m,
                            ProductName = _context.SanPhams
                                .Where(p => p.MaSanPham == item.MaSanPham)
                                .Select(p => p.TenSanPham)
                                .FirstOrDefault() ?? "Sản phẩm không xác định",
                            ImageUrl = _context.SanPhams
                                .Where(p => p.MaSanPham == item.MaSanPham)
                                .SelectMany(p => p.AnhSanPhams)
                                .Select(a => a.UrlAnh)
                                .FirstOrDefault() ?? "/images/default-product.jpg"
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (customer == null || customer.CartItems.Count == 0)
            {
                ViewBag.NoItemsAlert = true;
                return View(customer);
            }

            customer.Total = customer.CartItems.Sum(i => i.Price * i.Quantity);
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                try
                {
                    var maKhachHang = HttpContext.Session.GetString("User");
                    if (string.IsNullOrEmpty(maKhachHang))
                    {
                        TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiếp tục.";
                        return RedirectToAction("Index", "Cart");
                    }

                    // Create new invoice ID
                    var lastInvoice = _context.HoaDons.OrderByDescending(h => h.MaHoaDon).FirstOrDefault();
                    string newInvoiceId = "HD" + ((lastInvoice != null ? int.Parse(lastInvoice.MaHoaDon.Substring(2)) : 0) + 1).ToString("D3");

                    // Insert new invoice
                    var hoaDon = new HoaDon
                    {
                        MaHoaDon = newInvoiceId,
                        MaKhachHang = maKhachHang,
                        NgayXuatHoaDon = DateOnly.FromDateTime(DateTime.Now),
                        TongTien = model.Total,
                        TrangThai = model.PaymentMethod == "COD" ? 0 : 1 // 0: Pending, 1: Paid
                    };
                    _context.HoaDons.Add(hoaDon);

                    // Insert invoice details
                    foreach (var item in model.CartItems)
                    {
                        var chiTietHoaDon = new ChiTietHoaDon
                        {
                            MaHoaDon = newInvoiceId,
                            MaSanPham = item.ProductId,
                            SoLuong = item.Quantity,
                            Gia = item.Price
                        };
                        _context.ChiTietHoaDons.Add(chiTietHoaDon);
                    }

                    await _context.SaveChangesAsync();

                    // Redirect to VNPay if selected
                    if (model.PaymentMethod == "VNPay")
                    {
                        string callbackUrl = Url.Action("VNPayPaymentConfirmation", "Checkout", new { orderId = newInvoiceId, success = true }, Request.Scheme);
                        return Redirect(callbackUrl);
                    }

                    // Clear cart after successful order
                    await ClearCart(maKhachHang);

                    TempData["SuccessMessage"] = "Đặt hàng thành công! Đơn hàng của bạn đang được xử lý.";
                    return RedirectToAction("OrderConfirmation", "Checkout", new { orderId = newInvoiceId });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình đặt hàng. Vui lòng thử lại sau.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            return View(model);
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
