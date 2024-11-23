﻿using Microsoft.AspNetCore.Mvc;
using ASM_GS.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore; // Để sử dụng các phương thức của EF Core

namespace ASM_GS.Controllers
{
    public class DonHangLSController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonHangLSController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách đơn hàng
        public IActionResult Index()
        {
            // Fetching the list of orders for the logged-in user
            string userAccount = HttpContext.Session.GetString("UserAccount");
            string userId = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(userAccount) || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "LoginAndSignUp"); // Redirect if session is invalid
            }

            var orders = _context.DonHangs
                .Where(h => h.MaKhachHang == userId) // Filtering by user ID
                .Select(h => new DonHang_LSViewModel
                {
                    MaHoaDon = h.MaDonHang,
                    MaKhachHang = h.MaKhachHang,
                    TenKhachHang = _context.KhachHangs
                                  .Where(k => k.MaKhachHang == h.MaKhachHang)
                                  .Select(k => k.TenKhachHang)
                                  .FirstOrDefault(),
                    NgayXuatHoaDon = h.NgayDatHang,
                    TongTien = h.TongTien,
                    TrangThai = h.TrangThai
                }).ToList(); // Ensure you are returning a List<DonHang_LSViewModel>

            return View(orders); // Pass the collection to the view
        }


        // Hiển thị chi tiết một đơn hàng
        public IActionResult Details(string id)
        {
            var order = _context.DonHangs
                .Include(h => h.ChiTietDonHangs) // Eager loading các chi tiết đơn hàng
                .Where(h => h.MaDonHang == id) // Lọc theo mã đơn hàng
                .Select(h => new DonHang_LSViewModel
                {
                    MaHoaDon = h.MaDonHang,
                    MaKhachHang = h.MaKhachHang,
                    TenKhachHang = _context.KhachHangs
                                  .Where(k => k.MaKhachHang == h.MaKhachHang)
                                  .Select(k => k.TenKhachHang)
                                  .FirstOrDefault(),
                    NgayXuatHoaDon = h.NgayDatHang, // Sử dụng NgayDatHang (Ngày đặt hàng)
                    TongTien = h.TongTien,
                    TrangThai = h.TrangThai,
                    ChiTietHoaDons = h.ChiTietDonHangs.Select(ct => new ChiTietHoaDon_LSViewMode
                    {
                        MaSanPham = ct.MaSanPham,
                        MaCombo = ct.MaCombo,
                        SoLuong = ct.SoLuong,
                        Gia = ct.Gia,
                        SanPhamName = _context.SanPhams
                                            .Where(s => s.MaSanPham == ct.MaSanPham)
                                            .Select(s => s.TenSanPham)
                                            .FirstOrDefault(),
                        ComboName = _context.Combos
                                            .Where(c => c.MaCombo == ct.MaCombo)
                                            .Select(c => c.TenCombo)
                                            .FirstOrDefault(),
                        // Lấy hình ảnh sản phẩm từ bảng AnhSanPham
                        HinhAnhSanPham = _context.AnhSanPhams
                                                .Where(a => a.MaSanPham == ct.MaSanPham)
                                                .Select(a => a.UrlAnh) // Giả sử bảng AnhSanPham có trường HinhAnh chứa đường dẫn hình ảnh
                                                .FirstOrDefault(),
                        // Lấy hình ảnh combo từ bảng Combo
                        HinhAnhCombo = _context.Combos
                                                .Where(c => c.MaCombo == ct.MaCombo)
                                                .Select(c => c.Anh) // Trường HinhAnh chứa đường dẫn hình ảnh của combo
                                                .FirstOrDefault()
                    }).ToList()
                })
                .SingleOrDefault(); // Trả về một đối tượng duy nhất hoặc null nếu không tìm thấy

            if (order == null)
            {
                return NotFound(); // Nếu không tìm thấy đơn hàng, trả về lỗi 404
            }

            return View(order); // Truyền chi tiết đơn hàng vào view
        }



    }
}