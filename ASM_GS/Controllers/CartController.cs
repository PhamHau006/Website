using ASM_GS.Models;
using ASM_GS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

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
                    ComboId = item.MaCombo,
                    Quantity = item.SoLuong,
                    Price = item.MaSanPham != null
                        ? _context.SanPhams.Where(p => p.MaSanPham == item.MaSanPham).Select(p => (decimal?)p.Gia).FirstOrDefault() ?? 0m
                        : _context.Combos.Where(c => c.MaCombo == item.MaCombo).Select(c => (decimal?)c.Gia).FirstOrDefault() ?? 0m,
                    ProductName = item.MaSanPham != null
                        ? _context.SanPhams.Where(p => p.MaSanPham == item.MaSanPham).Select(p => p.TenSanPham).FirstOrDefault() ?? "Sản phẩm không xác định"
                        : _context.Combos.Where(c => c.MaCombo == item.MaCombo).Select(c => c.TenCombo).FirstOrDefault() ?? "Combo không xác định",
                    ImageUrl = item.MaSanPham != null
                        ? _context.SanPhams.Where(p => p.MaSanPham == item.MaSanPham).SelectMany(p => p.AnhSanPhams).Select(a => a.UrlAnh).FirstOrDefault() ?? "/images/default-product.jpg"
                        : $"/img/AnhCombo/{_context.Combos.Where(c => c.MaCombo == item.MaCombo).Select(c => c.Anh).FirstOrDefault()}"
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
        public async Task<IActionResult> AddToCart(string productId, string comboId, int quantity)
        {
            var maKhachHang = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(maKhachHang))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng." });
            }

            var gioHang = await GetOrCreateCartAsync(maKhachHang);

            if (!string.IsNullOrEmpty(productId))
            {
                // Thêm sản phẩm
                var product = await _context.SanPhams.FirstOrDefaultAsync(p => p.MaSanPham == productId);
                if (product == null || quantity <= 0 || quantity > product.SoLuong)
                {
                    return Json(new { success = false, message = $"Sản phẩm không tồn tại hoặc số lượng không hợp lệ." });
                }

                var cartItem = gioHang.ChiTietGioHangs.FirstOrDefault(ct => ct.MaSanPham == productId);
                if (cartItem != null)
                {
                    cartItem.SoLuong += quantity;
                }
                else
                {
                    cartItem = new ChiTietGioHang
                    {
                        MaGioHang = gioHang.MaGioHang,
                        MaSanPham = productId,
                        SoLuong = quantity
                    };
                    _context.ChiTietGioHangs.Add(cartItem);
                }
            }
            else if (!string.IsNullOrEmpty(comboId))
            {
                // Thêm combo
                var combo = await _context.Combos
                    .Include(c => c.ChiTietCombos)
                    .ThenInclude(ct => ct.MaSanPhamNavigation)
                    .FirstOrDefaultAsync(c => c.MaCombo == comboId);

                if (combo == null || quantity <= 0)
                {
                    return Json(new { success = false, message = "Combo không tồn tại hoặc số lượng không hợp lệ." });
                }

                foreach (var chiTiet in combo.ChiTietCombos)
                {
                    var sanPham = chiTiet.MaSanPhamNavigation;
                    if (sanPham == null || sanPham.SoLuong < chiTiet.SoLuong * quantity)
                    {
                        return Json(new { success = false, message = $"Sản phẩm '{sanPham?.TenSanPham}' trong combo không đủ số lượng." });
                    }
                }

                var cartItem = gioHang.ChiTietGioHangs.FirstOrDefault(ct => ct.MaCombo == comboId);
                if (cartItem != null)
                {
                    cartItem.SoLuong += quantity;
                }
                else
                {
                    cartItem = new ChiTietGioHang
                    {
                        MaGioHang = gioHang.MaGioHang,
                        MaCombo = comboId,
                        SoLuong = quantity
                    };
                    _context.ChiTietGioHangs.Add(cartItem);
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã thêm vào giỏ hàng." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int itemId, int quantity)
        {
            var cartItem = await _context.ChiTietGioHangs.FindAsync(itemId);
            if (cartItem == null || quantity <= 0)
            {
                return Json(new { success = false, message = "Invalid item or quantity." });
            }

            if (cartItem.MaSanPham != null)
            {
                var product = await _context.SanPhams.FindAsync(cartItem.MaSanPham);
                if (product == null || quantity > product.SoLuong)
                {
                    return Json(new { success = false, message = $"Số lượng không được vượt quá {product?.SoLuong ?? 0}" });
                }
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

        private async Task<GioHang> GetOrCreateCartAsync(string maKhachHang)
        {
            var gioHang = await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefaultAsync(g => g.MaKhachHang == maKhachHang);

            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    MaGioHang = "GH" + Guid.NewGuid().ToString("N"),
                    MaKhachHang = maKhachHang,
                    NgayTao = DateOnly.FromDateTime(DateTime.Today),
                };
                _context.GioHangs.Add(gioHang);
                await _context.SaveChangesAsync();
            }

            return gioHang;
        }
    }
}
