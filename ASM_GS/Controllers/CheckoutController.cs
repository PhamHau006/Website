using ASM_GS.Models;
using ASM_GS.Services;
using ASM_GS.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
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
    .Select(item => new CartItemViewModel
    {
        ItemId = item.Id,
        ProductId = item.MaSanPham, // Có thể null
        ComboId = item.MaCombo, // Có thể null
        Quantity = item.SoLuong,
        Price = item.SanPham != null
                ? item.SanPham.Gia
                : (item.Combo != null ? item.Combo.Gia : 0), // Xử lý null
        ProductName = item.SanPham != null
                      ? item.SanPham.TenSanPham
                      : (item.Combo != null ? item.Combo.TenCombo : "Không xác định"),
        ImageUrl = item.SanPham != null && item.SanPham.AnhSanPhams.Any()
                   ? item.SanPham.AnhSanPhams.FirstOrDefault().UrlAnh
                   : (item.Combo != null ? $"/img/AnhCombo/{item.Combo.Anh}" : "/images/default-product.jpg")
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
        public async Task<IActionResult> Index(CheckoutViewModel model, string payment = "COD")
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
                    .Select(item => new CartItemViewModel
                    {
                        ItemId = item.Id,
                        ProductId = item.MaSanPham,
                        ComboId = item.MaCombo,
                        Quantity = item.SoLuong,
                        Price = item.SanPham != null
                            ? item.SanPham.Gia
                            : (item.Combo != null ? item.Combo.Gia : 0),
                        ProductName = item.SanPham != null
                            ? item.SanPham.TenSanPham
                            : (item.Combo != null ? item.Combo.TenCombo : "Không xác định"),
                        ImageUrl = item.SanPham != null && item.SanPham.AnhSanPhams.Any()
                            ? item.SanPham.AnhSanPhams.FirstOrDefault().UrlAnh
                            : (item.Combo != null ? $"/img/AnhCombo/{item.Combo.Anh}" : "/images/default-product.jpg")
                    }).ToListAsync();
            }

            decimal totalAmount = 0;
            if (TempTong == 0)
            {
                totalAmount = model.CartItems.Sum(i => i.Price * i.Quantity);
            }
            else
            {
                totalAmount = (decimal)TempTong;
            }

            // Tạo mã hóa đơn mới
            var lastInvoice = await _context.HoaDons.OrderByDescending(h => h.MaHoaDon).FirstOrDefaultAsync();
            string newInvoiceId = "HD" + ((lastInvoice != null ? int.Parse(lastInvoice.MaHoaDon.Substring(2)) : 0) + 1).ToString("D3");

            // Tạo mã đơn hàng mới
            var lastOrder = await _context.DonHangs.OrderByDescending(d => d.MaDonHang).FirstOrDefaultAsync();
            string newOrderId = "DH" + ((lastOrder != null ? int.Parse(lastOrder.MaDonHang.Substring(2)) : 0) + 1).ToString("D3");

            // Tạo hóa đơn
            var hoaDon = new HoaDon
            {
                MaHoaDon = newInvoiceId,
                MaKhachHang = maKhachHang,
                NgayXuatHoaDon = DateOnly.FromDateTime(DateTime.Now),
                TongTien = totalAmount,
                TrangThai = payment == "COD" ? 0 : 1 // 0 = Chưa thanh toán, 1 = Đã thanh toán
            };
            MaHoaDonTemp = newInvoiceId;
            _context.HoaDons.Add(hoaDon);
            await _context.SaveChangesAsync();

            // Tạo đơn hàng
            var donHang = new DonHang
            {
                MaDonHang = newOrderId,
                MaKhachHang = maKhachHang,
                NgayDatHang = DateOnly.FromDateTime(DateTime.Now),
                TrangThai = 0, // Đang xử lý
                TongTien = totalAmount
            };
            MaDonhangTemp = newOrderId;
            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            // Lưu các chi tiết đơn hàng và chi tiết hóa đơn
            foreach (var item in model.CartItems)
            {
                if (!string.IsNullOrEmpty(item.ProductId)) // Nếu là sản phẩm
                {
                    // Thêm vào chi tiết đơn hàng
                    var chiTietDonHang = new ChiTietDonHang
                    {
                        MaDonHang = donHang.MaDonHang,
                        MaSanPham = item.ProductId,
                        SoLuong = item.Quantity,
                        Gia = item.Price
                    };
                    _context.ChiTietDonHangs.Add(chiTietDonHang);

                    // Thêm vào chi tiết hóa đơn
                    var chiTietHoaDon = new ChiTietHoaDon
                    {
                        MaHoaDon = hoaDon.MaHoaDon,
                        MaSanPham = item.ProductId,
                        SoLuong = item.Quantity,
                        Gia = item.Price
                    };
                    _context.ChiTietHoaDons.Add(chiTietHoaDon);

                    // Cập nhật số lượng sản phẩm trong kho
                    var sanPham = await _context.SanPhams.FirstOrDefaultAsync(p => p.MaSanPham == item.ProductId);
                    if (sanPham != null)
                    {
                        sanPham.SoLuong -= item.Quantity;
                        _context.SanPhams.Update(sanPham);
                    }
                }
                else if (!string.IsNullOrEmpty(item.ComboId)) // Nếu là combo
                {
                    // Thêm vào chi tiết hóa đơn cho combo
                    var chiTietHoaDon = new ChiTietHoaDon
                    {
                        MaHoaDon = hoaDon.MaHoaDon,
                        MaCombo = item.ComboId,
                        SoLuong = item.Quantity,
                        Gia = item.Price
                    };
                    _context.ChiTietHoaDons.Add(chiTietHoaDon);

                    // Thêm vào chi tiết đơn hàng cho combo
                    var chiTietDonHang = new ChiTietDonHang
                    {
                        MaDonHang = donHang.MaDonHang,
                        MaCombo = item.ComboId,
                        SoLuong = item.Quantity,
                        Gia = item.Price
                    };
                    _context.ChiTietDonHangs.Add(chiTietDonHang);
                }
            }

            await _context.SaveChangesAsync();

            // Xoá giỏ hàng của khách hàng sau khi thanh toán thành công
            await ClearCart(maKhachHang);

            // Thanh toán thành công, chuyển hướng đến trang xác nhận đơn hàng
            TempData["SuccessMessage"] = "Thanh toán thành công! Đơn hàng của bạn đang được xử lý.";

            // Nếu thanh toán qua VNPay
            if (payment == "VNPay")
            {
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

            // Chuyển hướng đến trang xác nhận đơn hàng
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
                // Xóa chi tiết giỏ hàng
                _context.ChiTietGioHangs.RemoveRange(gioHang.ChiTietGioHangs);
                // Xóa giỏ hàng
                _context.GioHangs.Remove(gioHang);

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
                    discountCodeEntity.IsUsed = true;
                    await _context.SaveChangesAsync();
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

            // Lấy hóa đơn từ cơ sở dữ liệu
            var hoaDon = await _context.HoaDons.FirstOrDefaultAsync(a => a.MaHoaDon == MaHoaDonTemp);
            if (hoaDon != null)
            {
                hoaDon.TrangThai = 1; // Đánh dấu hóa đơn đã thanh toán
                await _context.SaveChangesAsync();
            }

            // Lấy đơn hàng từ cơ sở dữ liệu
            var donHang = await _context.DonHangs.FirstOrDefaultAsync(b => b.MaDonHang == MaDonhangTemp);

            // Cập nhật số lượng sản phẩm trong kho sau khi thanh toán thành công
            var cartItems = await _context.GioHangs
                .Where(g => g.MaKhachHang == HttpContext.Session.GetString("User"))
                .SelectMany(g => g.ChiTietGioHangs)
                .ToListAsync();

            foreach (var item in cartItems)
            {
                // Tìm sản phẩm trong kho
                var product = await _context.SanPhams.FirstOrDefaultAsync(p => p.MaSanPham == item.MaSanPham);
                if (product != null)
                {
                    // Giảm số lượng sản phẩm trong kho
                    product.SoLuong -= item.SoLuong;
                    _context.SanPhams.Update(product);
                }
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Xoá giỏ hàng sau khi thanh toán thành công
            var maKhachHang = HttpContext.Session.GetString("User");
            if (!string.IsNullOrEmpty(maKhachHang))
            {
                await ClearCart(maKhachHang); // Xoá giỏ hàng của khách hàng hiện tại
            }

            TempData["SuccessMessage"] = "Thanh toán VNPay thành công! Đơn hàng của bạn đang được xử lý.";
            return RedirectToAction("OrderConfirmation", new { orderId = donHang.MaDonHang });
        }


    }
}
