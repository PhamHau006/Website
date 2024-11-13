using ASM_GS.Models;
using ASM_GS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ASM_GS.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy mã khách hàng từ session
            var maKhachHang = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(maKhachHang))
            {
                return RedirectToAction("Index", "LoginAndSignUp");
            }

            // Lấy chi tiết giỏ hàng của khách hàng hiện tại
            var cartItems = await _context.GioHangs
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
                })
                .ToListAsync();

            // Tạo `CartViewModel` và truyền dữ liệu vào view
            var cart = new CartViewModel
            {
                Items = cartItems
            };

            return View(cart);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int itemId, int quantity)
        {
            var cartItem = await _context.ChiTietGioHangs.FindAsync(itemId);
            if (cartItem == null || quantity <= 0)
            {
                return Json(new { success = false, message = "Invalid item or quantity." });
            }

            var product = await _context.SanPhams.FindAsync(cartItem.MaSanPham);
            if (product == null || quantity > product.SoLuong)
            {
                return Json(new { success = false, message = $"Số lượng không được vượt quá {product?.SoLuong ?? 0}" });
            }

            cartItem.SoLuong = quantity;
            _context.ChiTietGioHangs.Update(cartItem);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Quantity updated successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var cartItem = await _context.ChiTietGioHangs.FindAsync(id);
            if (cartItem != null)
            {
                _context.ChiTietGioHangs.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
