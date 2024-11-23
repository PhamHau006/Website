using ASM_GS.Models;
using ASM_GS.Services;
using ASM_GS.ViewModels;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IVnPayServices _vnPayServices;

        public CheckoutController(ApplicationDbContext context, IVnPayServices vnPayServices)
        {
            _context = context;
            _vnPayServices = vnPayServices;
        }
        // Display checkout page
        public static string MaHoaDonTemp;
        public static string MaDonhangTemp;
        public static float TempTong;
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
        public async Task<IActionResult> Index(CheckoutViewModel model,string payment="COD")
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
            decimal totalAmount = 0;
            if (TempTong==0)
            {
                totalAmount = model.CartItems.Sum(i => i.Price * i.Quantity);
            }
            else
                totalAmount = (decimal)TempTong;
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
            MaHoaDonTemp = newInvoiceId;
            _context.HoaDons.Add(hoaDon);

            var donHang = new DonHang
            {
                MaDonHang = newOrderId,
                MaKhachHang = maKhachHang,
                NgayDatHang = DateOnly.FromDateTime(DateTime.Now),
                TrangThai = 0,
                TongTien = totalAmount
            };
            MaDonhangTemp = newOrderId;
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
            if (payment == "VNPay")
            {
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
                var vnPayModel = new VnPaymentRequestModel
                {
                    Amount = (float)totalAmount,
                    CreatedDate = DateTime.Now,
                    Description = $"{model.SoDienThoai} {model.TenKhachHang}",
                    FullName = model.TenKhachHang,
                    OrderId = (new Random().Next(1000, 100000)).ToString()
                };
                return Redirect(_vnPayServices.CreatePaymentUrl(HttpContext, vnPayModel));
            }
            TempData["SuccessMessage"] = "Đặt hàng thành công!";
            return RedirectToAction("OrderConfirmation", new { orderId = donHang.MaDonHang });
        }




        //[HttpGet]
        //public async Task<IActionResult> VNPayPaymentConfirmation(string orderId, bool success)
        //{
        //    if (success)
        //    {
        //        var hoaDon = await _context.HoaDons.FirstOrDefaultAsync(h => h.MaHoaDon == orderId);
        //        if (hoaDon != null)
        //        {
        //            hoaDon.TrangThai = 1; // Mark as paid
        //            await _context.SaveChangesAsync();

        //            TempData["SuccessMessage"] = "Thanh toán VNPay thành công! Đơn hàng của bạn đang được xử lý.";
        //            return RedirectToAction("OrderConfirmation", "Checkout", new { orderId = orderId });
        //        }
        //    }

        //    TempData["ErrorMessage"] = "Thanh toán VNPay không thành công. Vui lòng thử lại.";
        //    return RedirectToAction("Index", "Cart");
        //}

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






        //Nghi code
        [HttpPost]
        public async Task<JsonResult> ApplyDiscount(string discountCode, decimal totalAmount)
        {
            if (string.IsNullOrEmpty(discountCode))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá." });
            }

            // Lấy mã nhập giảm giá
            var discountCodeEntity = await _context.MaNhapGiamGias
                .FirstOrDefaultAsync(c => c.MaNhap == discountCode && !c.IsUsed);

            if (discountCodeEntity != null)
            {
                // Lấy thông tin giảm giá
                var discount = await _context.GiamGia
                    .FirstOrDefaultAsync(d => d.MaGiamGia == discountCodeEntity.MaGiamGia);

                if (discount != null)
                {
                    // Kiểm tra trạng thái của giảm giá
                    if (discount.TrangThai != 1) // Giả sử trạng thái 1 là "áp dụng"
                    {
                        return Json(new { success = false, message = "Mã giảm giá đã hết hạn sử dụng" });
                    }

                    // Tính số tiền giảm giá
                    decimal discountAmount = (discount.GiaTri / 100) * totalAmount;
                    TempTong = (float)(totalAmount - discountAmount);
                    return Json(new
                    {
                        success = true,
                        discountAmount,
                        finalTotal = totalAmount - discountAmount
                    });
                }
            }          
            return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã được sử dụng." });
        }













        public async Task<IActionResult> PaymentCallBack()
        {
            // Xử lý phản hồi từ VNPay
            var response = _vnPayServices.PaymentExcute(Request.Query);
            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["ErrorMessage"] = "Thanh toán VNPay không thành công. Vui lòng thử lại.";
                return RedirectToAction("Index", "Checkout"); 
            }

            var hoaDon = _context.HoaDons.FirstOrDefault(a => a.MaHoaDon == MaHoaDonTemp);
            if (hoaDon != null)
            {
                hoaDon.TrangThai = 1; 
                await _context.SaveChangesAsync();
            }
            var donHang = _context.DonHangs.FirstOrDefault(b => b.MaDonHang == MaDonhangTemp);
            TempData["SuccessMessage"] = "Thanh toán VNPay thành công! Đơn hàng của bạn đang được xử lý.";
            return RedirectToAction("OrderConfirmation", new { orderId = donHang }); 
        }

    }
}